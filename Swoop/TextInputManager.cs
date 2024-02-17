using MGRawInputLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

namespace SwoopLib {    
    public class TextLine {        
        string _text;
        XYPair _size_px;
        int _length;

        public string text {
            get => _text;
            set {
                if (_text != value) {
                    _text = value;

                    _size_px = Drawing.measure_string_profont_xy(value);
                    _length = _text.Length;
                }
            }
        }

        public int length => _length;
        public XYPair size_px => _size_px;

        TextInputManager parent;
        int line_index => parent.get_index(this);

        public void insert_text(int position, string text) {
            this.text = _text.Insert(position, text);
        }

        public void delete_text_no_update(int start, int count) {
            _text = _text.Remove(start, count);
        }
        public void update_size_length() {
            _size_px = Drawing.measure_string_profont_xy(_text);
            _length = _text.Length;
        }

        public TextLine(TextInputManager parent) {
            this.parent = parent;
            text = string.Empty;
        }
        public TextLine(TextInputManager parent, string text) {
            this.parent = parent;
            this.text = text;
        }

        public static TextLine operator +(TextLine a, TextLine b) { 
            b.parent = a.parent; 
            return new TextLine(a.parent, a.text + b.text); 
        }
    }


    public class TextInputManager {
        public bool input_enabled = true;

        public bool multiline = true;
        
        public int longest_line_text_length = 0;
        public int longest_line_index = -1;        

        bool insert_mode = false;

        XYPair previous_cursor_pos = XYPair.Zero;
        public XYPair cursor_pos = XYPair.Zero;
        int cursor_pos_stored_X = 0;

        //selection handling
        XYPair _selection_start = XYPair.MinusOne;
        XYPair _selection_end = XYPair.MinusOne;

        public XYPair selection_start => _selection_start;
        public XYPair selection_end => _selection_end;

        public enum selection_shape { LINEAR, BLOCK };
        public selection_shape select_shape = selection_shape.LINEAR;

        public bool has_selection() {
            if (_selection_start == XYPair.MinusOne) return false;
            if (_selection_end == XYPair.MinusOne) return false;
            return _selection_start != _selection_end;
        }

        public void start_selection() {
            _selection_start = cursor_pos;
        }
        public void end_selection() {
            _selection_end = cursor_pos;
        }
        public void clear_selection() {
            _selection_start = XYPair.MinusOne;
            _selection_end = XYPair.MinusOne;
        }

        public (XYPair min, XYPair max) get_actual_selection_min_max() {
            XYPair min, max;
            if (selection_start.X < selection_end.X) {
                min.X = selection_start.X;
                max.X = selection_end.X;
            } else {
                min.X = selection_end.X;
                max.X = selection_start.X;
            }

            if (selection_start.Y < selection_end.Y) {
                min.Y = selection_start.Y;
                max.Y = selection_end.Y;
            } else {
                min.Y = selection_end.Y;
                max.Y = selection_start.Y;
            }

            return (min, max);
        }

        //text handling
        public List<TextLine> lines = new List<TextLine>();
        public TextLine current_line => lines[cursor_pos.Y];
        public string current_line_text => lines[cursor_pos.Y].text;
        public int current_line_text_length => lines[cursor_pos.Y].text.Length;

        public int line_count => lines.Count;
        public int cursor => cursor_pos.X;
        public int current_line_index => cursor_pos.Y;

        InputHandler input_handler = new InputHandler();

        bool captured = true;

        bool capture_tab = true;
        //bool uncapture_on_escape;

        public Action<InputHandler> external_input_handler;

        public TextInputManager() {
            lines.Add(new TextLine(this, ""));
        }

        public TextInputManager(string text) {

            insert_text(text);
            cursor_pos = XYPair.Zero;

            return;
            StringReader sr = new StringReader(text);
            while (sr.Peek() > -1) {
                var l = sr.ReadLine();
                lines.Add(new TextLine(this, l));
            }
        }

        public void update_input() {
            previous_cursor_pos = cursor_pos;
            input_handler.update();

            if (external_input_handler != null) external_input_handler(input_handler);

            if (!Window.is_active) return;
            if (!captured) return;

            foreach (Input.KeyTime key_time in input_handler.just_pressed_keys) {
                if (!key_time.handled)
                    eval_key(key_time);
            }

            foreach (Input.KeyTime key_time in input_handler.pressed_keys) {
                if ((key_time.held && !key_time.handled) && (key_time.repeat()))
                    eval_key(key_time);
            }

        }

        internal int get_index(TextLine text_line) {
            for (int i = 0; i < lines.Count; i++) {
                if (lines[i] == text_line) return i;
            }
            return -1;
        }

        internal bool try_get_index(TextLine text_line, out int index) {
            for (int i = 0; i < lines.Count; i++) {
                if (lines[i] == text_line) {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        void insert_text(string text) {
            if (has_selection()) {
                delete_selected_text();
            }

            if (!multiline) {
                StringReader sr = new StringReader(text);

                int c = 0;
                int p = 0;
                string line = "";

                while (p > -1) {
                    line = sr.ReadLine();
                    p = sr.Peek();

                    lines[0].insert_text(cursor, line);
                    cursor_pos.X += line.Length;
                }


                validate_cursor();
                store_cursor_X();

            } else {

                StringReader sr = new StringReader(text);

                string line = "";

                int c = 0;
                int p = 0;
                while (p > -1) {
                    line = sr.ReadLine();
                    p = sr.Peek();

                    if (c == 0) {
                        //first line
                        if (lines.Count == 0) lines.Add(new TextLine(this));
                        lines[cursor_pos.Y].insert_text(cursor, line);

                        cursor_pos.X += line.Length;

                        if (p > -1) //more lines to come                    
                            insert_newline();

                    } else if (p > -1) {
                        //middle lines
                        lines.Insert(cursor_pos.Y + c - 1, new TextLine(this, line));

                    } else if (p == -1) {
                        //bottom line
                        lines.Insert(cursor_pos.Y + c - 1, new TextLine(this, line));

                        lines[cursor_pos.Y + c - 1].text += lines[cursor_pos.Y + c].text;
                        lines.RemoveAt(cursor_pos.Y + c);

                        cursor_pos.X = line.Length;
                        break;
                    }

                    c++;
                }

                cursor_pos.Y += c - 1;

                validate_cursor();
                store_cursor_X();

            }
        }

        void insert_newline() {
            if (has_selection()) {
                clear_selection();
            }

            validate_cursor();

            string cursor_to_end = current_line_text.Substring(cursor);
            var sub_len = cursor_to_end.Length;

            delete_text_at_cursor(sub_len);
            Debug.Print(cursor_to_end + " " + sub_len);
            lines.Insert(current_line_index + 1, new TextLine(this, cursor_to_end));

            cursor_pos.Y++; cursor_pos.X = 0;
            validate_cursor();

            store_cursor_X();
        }
        
        void backspace() {
            delete_text_at_cursor(-1);            
        }
        void backspace_word() {
            delete_text_at_cursor(-find_word_size_left_of_cursor());
        }

        void delete() {
            delete_text_at_cursor(1);
        }
        void delete_word() {
            var ws = find_word_size_right_of_cursor();
            Debug.WriteLine(ws);
            delete_text_at_cursor(ws);
        }

        void delete_selected_text() {
            Debug.Write(get_selected_text());
            var sel = get_actual_selection_min_max();

            var top_line = sel.min.Y;
            var bottom_line = sel.max.Y;
            var selected_lines = bottom_line - top_line;

            var top_line_x = (selection_start.Y < selection_end.Y) ? selection_start.X : selection_end.X;
            var bottom_line_x = (selection_start.Y > selection_end.Y) ? selection_start.X : selection_end.X;

            var single_line_select = top_line == bottom_line;

            if (single_line_select) {
                delete_text(sel.min.Y, sel.min.X, (sel.max.X - sel.min.X));

                clear_selection();
                cursor_pos = sel.min;

            } else {                
                //cursor isn't at the start of the top line, so delete from cursor until the end
                if (top_line_x > 0)
                    delete_text(top_line, top_line_x, lines[top_line].text.Length - top_line_x);
                //cursor is at the start of the top line, so remove the whole line
                //line also actually has text on it
                else if (lines[top_line].text.Length > 0) {
                    top_line--;
                    selected_lines++;
                }

                //similar but in reverse for the bottom line, if the selection X is at the end of the line, remove the whole line
                if (bottom_line_x < lines[bottom_line].text.Length)
                    delete_text(bottom_line, 0, bottom_line_x);
                else if (lines[bottom_line].text.Length > 0) {
                    selected_lines++;
                }

                //go ahead and actually do the line removal
                lock (lines) {
                    if (selected_lines >= 2) {
                        for (int i = 1; i <= selected_lines - 1; i++) {
                            //Debug.WriteLine($"line {i}");
                            lines.RemoveAt(top_line + 1); 
                            if (longest_line_index == top_line + 1) longest_line_index = -1;
                        }

                        if ((lines.Count > 1 && top_line > -1 && lines.Count > top_line + 1) /*&& (!String.IsNullOrEmpty(lines[top_line+1].text))*/) {
                            lines[top_line].text += lines[top_line + 1].text;
                            lines.RemoveAt(top_line + 1);

                            if (longest_line_index == top_line + 1) longest_line_index = -1;
                        }
                    }

                }                

                clear_selection();
                cursor_pos = new XYPair(top_line_x, sel.min.Y);
            }

            validate_cursor();
        }

        void delete_text_at_cursor(int count) {
            if (has_selection()) {
                delete_selected_text();
            } else {
                delete_text(cursor_pos.Y, cursor_pos.X, count);
            }

            validate_cursor();
        }

        public void delete_text(int line, int start, int count, bool auto_remove_lines = true) {
            lock (lines) {
                //stupid loop to handle deleting text within the TextLine system
                if (count < 0) {
                    var c = Math.Abs(count);
                    for (int i = 0; i < c; i++) {
                        //at the start of a line so instead of removing a character
                        //we need to move the current line's text to the above line
                        //and delete the current one
                        if (start - i <= 0) {
                            if (!auto_remove_lines) break;
                            if (line <= 0) break;
                            var t = lines[line].text;

                            cursor_pos.Y--;
                            lines.RemoveAt(line);
                            if (longest_line_index == line) longest_line_index = -1;

                            line--;

                            cursor_pos.X = lines[line].length;
                            start = cursor_pos.X + 1;

                            lines[line].text += t;

                        } else {
                            lines[line].delete_text_no_update(start - 1 - i, 1);
                            cursor_pos.X--;
                        }

                        lines[line].update_size_length();
                        validate_cursor();
                    }

                } else {                    
                    for (int i = 0; i < count; i++) {
                        if (start + i >= lines[line].length) {
                            //Debug.Write($"L");
                            if (!auto_remove_lines) break;
                            if (line + 1 >= line_count) break;
                            lines[line].text += lines[line + 1].text;
                            lines[line].update_size_length();

                            lines.RemoveAt(line + 1);
                            if (longest_line_index == line) longest_line_index = -1;
                            //Debug.Write($"E");

                        } else {
                            lines[line].delete_text_no_update(start, 1);
                            //Debug.Write($" C{start + i}");
                        }
                    }

                    lines[line].update_size_length();
                    //Debug.Write($"\n");
                }

            }
            store_cursor_X();
        }

        //Cut/copy/paste
        string get_selected_text() {
            StringBuilder sb = new StringBuilder();

            var sel = get_actual_selection_min_max();

            var top_line = sel.min.Y;
            var bottom_line = sel.max.Y;

            var selected_lines = bottom_line - top_line;

            var top_line_x = (selection_start.Y <= selection_end.Y) ? selection_start.X : selection_end.X;
            var bottom_line_x = (selection_start.Y > selection_end.Y) ? selection_start.X : selection_end.X;

            var single_line_select = top_line == bottom_line;

            if (single_line_select) {
                sb.Append(lines[top_line].text.Substring(sel.min.X, sel.max.X - sel.min.X));

            } else {
                for (int i = 0; i < selected_lines; i++) {
                    int current_line = top_line + i;
                    var current_text = lines[current_line].text;

                    if (i == 0) {
                        //top line
                        sb.AppendLine(current_text.Substring(top_line_x, current_text.Length - top_line_x));

                    } else if (i > 0 && i < selected_lines) {
                        //middle lines
                        sb.AppendLine(current_text);

                    } else if (i == selected_lines) {
                        //bottom line
                        sb.Append(current_text.Substring(0, bottom_line_x));
                    }
                }
            }

            return sb.ToString();
        }

        void copy() {
            if (has_selection()) {
                string clipboard_text = get_selected_text();

                Externs.set_clipboard_string(clipboard_text);
                
            } else {
                if (cursor == 0) {
                    Externs.set_clipboard_string(current_line_text);

                } else {
                    int start, end;
                    find_word_at_cursor(out start, out end);

                    int cursor_pos_in_word = cursor - start;
                    string copy = current_line_text.Substring(start, end - start);


                    if (cursor_pos_in_word > 0 && separator_char_switch(copy[cursor_pos_in_word-1])) {
                        //right side
                        var c = copy[cursor_pos_in_word-1];
                        var split = copy.Split(c);

                        Externs.set_clipboard_string(split[1]);

                    } else if (cursor_pos_in_word < current_line_text_length-1
                        && separator_char_switch(copy[cursor_pos_in_word])) {

                        var c = copy[cursor_pos_in_word];
                        var split = copy.Split(c);

                        Externs.set_clipboard_string(split[0]);

                    } else {
                        if (!string.IsNullOrWhiteSpace(copy)) {
                            Externs.set_clipboard_string(copy);
                        }
                    }

                }
            }
             
        }

        void cut() {
            copy();
            delete_selected_text();
            store_cursor_X();
        }

        void paste() {
            string clipboard_text;

            if (Externs.get_clipboard_string(out clipboard_text)) {
                if (!string.IsNullOrEmpty(clipboard_text)) {
                    insert_text(clipboard_text);
                }

                store_cursor_X();
            }
        }

        void duplicate_line() {
            lines.Insert(current_line_index, new TextLine(this, current_line_text));
            cursor_pos.Y++;
        }

        //Cursor

        void select_region() {

        }
        void select_all() {            
            _selection_start = XYPair.Zero;
            _selection_end = new XYPair(lines[line_count-1].length, line_count-1);

            validate_cursor();
        }

        internal void validate_cursor() {
            if (lines.Count == 0) lines.Add(new TextLine(this));

            if (current_line_index < 0) cursor_pos.Y = 0;
            if (current_line_index >= line_count) cursor_pos.Y = line_count - 1;

            if (cursor_pos.X > current_line.length) {
                cursor_pos.X = current_line.length;
            }

            if (cursor_pos.X < 0) cursor_pos.X = 0;
            
            if (_selection_start.Y < 0) _selection_start.Y = 0;
            if (_selection_end.Y < 0) _selection_end.Y = 0;

            if (_selection_start.Y > line_count - 1) _selection_start.Y = line_count - 1;
            if (_selection_end.Y > line_count - 1) _selection_end.Y = line_count - 1;
            
            if (!multiline && line_count > 1) {
                lines.RemoveRange(1, line_count - 1);
            }

            find_longest_line(out longest_line_index, out longest_line_text_length);
        }

        void find_longest_line(out int index, out int length) {
            index = -1; length = int.MinValue;

            for (int i = 0; i < lines.Count; i++) {                
                if (lines[i].length > length) {
                    index = i;
                    length = lines[i].length;
                }
            }
        }


        void store_cursor_X() => cursor_pos_stored_X = cursor_pos.X;

        void cursor_up() { 
            cursor_pos.Y--;           

            validate_cursor();
            //return;
            if (current_line_text_length > cursor_pos_stored_X)
                cursor_pos.X = cursor_pos_stored_X;
            else {
                cursor_pos.X = current_line_text_length;
            }

            validate_cursor();
        }
        void cursor_down() { 
            cursor_pos.Y++;

            validate_cursor();
           //return;
            if (current_line_text_length > cursor_pos_stored_X)
                cursor_pos.X = cursor_pos_stored_X;
            else {
                cursor_pos.X = current_line_text_length;
            }

            validate_cursor();
        }
        void cursor_left() { 
            cursor_pos.X--;            
            validate_cursor();
        }
        void cursor_left(int c) {
            cursor_pos.X-=c;
            validate_cursor();
        }
        void cursor_right() { 
            cursor_pos.X++;
            validate_cursor();
        }
        void cursor_right(int c) {
            cursor_pos.X+=c;
            validate_cursor();
        }

        void page_up() {
            validate_cursor();
        }
        void page_down() {
            validate_cursor();
        }

        void cursor_home_line() {
            if (!input_handler.shift) clear_selection();
            if (input_handler.shift && !has_selection())
                start_selection();

            cursor_pos.X = 0;

            validate_cursor();

            if (input_handler.shift)
                end_selection();

            store_cursor_X();
        }
        void cursor_home_file() {
            if (!input_handler.shift) clear_selection();
            if (input_handler.shift && !has_selection())
                start_selection();

            cursor_pos = XYPair.Zero;

            validate_cursor();

            if (input_handler.shift)
                end_selection();

            store_cursor_X();
        }

        void cursor_end_line() {
            if (!input_handler.shift) clear_selection();
            if (input_handler.shift && !has_selection())
                start_selection();

            cursor_pos.X = current_line_text_length;

            validate_cursor();

            if (input_handler.shift)
                end_selection();

            store_cursor_X();
        }
        void cursor_end_file() {
            if (!input_handler.shift) clear_selection();
            if (input_handler.shift && !has_selection())
                start_selection();

            cursor_pos.Y = line_count - 1;

            validate_cursor();

            if (input_handler.shift)
                end_selection();

            store_cursor_X();
        }
        
        bool separator_char_switch(char c) {            
            switch (c) {
                case ' ':
                case '_':
                case '=': case '|':
                case '+': case '-': case '*':
                case '&': case '%': case '$': 
                case '#': case '@':
                case '/': case '\\':
                case '!': case '?':
                case '[': case ']':
                case '{': case '}':
                case ':': case ';':
                case ',': case '.':
                case '<': case '>':
                case '\'': case '\"':
                    return true;                        
            }
            return false;
        }

        void find_word_at_cursor(out int start, out int end) {
            int l = find_word_size_left_of_cursor();
            int r = find_word_size_right_of_cursor();
            
            start = cursor - l; end = start + l + r;
        }

        int find_word_size_left_of_cursor() {
            bool started_on_hit = false;
            int c = 1;

            while (cursor_pos.X - c > 0) {
                if (cursor_pos.X - c == current_line_text_length) { c++; continue; }
                char current = current_line_text[cursor_pos.X - c];
                
                Debug.Write(current);

                if (started_on_hit ? !separator_char_switch(current) : separator_char_switch(current)) {
                    if (c == 1) { started_on_hit = true; c++; continue; }
                    if (c == 2 && started_on_hit) { c++; started_on_hit = false; continue; }

                    Debug.Write($" {c-1}\n");
                    return c - 1;
                }

                c++;
            }

            Debug.Write($" {c}\n");
            return c;
        }

        int find_word_size_right_of_cursor() {
            bool started_on_hit = false;
            int c = 0;
            while (cursor_pos.X + c < current_line_text_length) {
                char current = current_line_text[cursor_pos.X+c];
                Debug.Write(current);

                if (started_on_hit ? !separator_char_switch(current) : separator_char_switch(current)) {

                    if (c == 0) { started_on_hit = true; c++; continue; } 
                    if (c == 1 && started_on_hit) { c++; started_on_hit = false; continue; }

                    Debug.Write($" {c}\n");
                    return c;
                }
                c++;
            }

            Debug.Write($" {c}\n");
            return c;
        }

        // haha, oh boy
        void eval_key(Input.KeyTime key) {
            switch (key.key) {
                case Keys.None: return;
                case Keys.Escape: return;

                //cursor movement
                case Keys.PageUp: break;
                case Keys.PageDown: break;

                case Keys.Home:
                    if (!input_handler.ctrl) cursor_home_line();
                    else cursor_home_file();
                    return;
                case Keys.End:
                    if (!input_handler.ctrl) cursor_end_line();
                    else cursor_end_file();
                    return;

                case Keys.Up:
                    if (!input_handler.shift) clear_selection();
                    if (input_handler.shift && !has_selection())
                        start_selection();

                    cursor_up();

                    if (input_handler.shift)
                        end_selection();

                    return;
                case Keys.Down:
                    if (!input_handler.shift) clear_selection();
                    if (input_handler.shift && !has_selection())
                        start_selection();

                    cursor_down();

                    if (input_handler.shift)
                        end_selection();
                    return;

                case Keys.Left:
                    if (!input_handler.shift) clear_selection();
                    if (input_handler.shift && !has_selection())
                        start_selection();

                    if (!input_handler.ctrl)
                        cursor_left(1);
                    else
                        cursor_left(find_word_size_left_of_cursor());

                    if (input_handler.shift)
                        end_selection();

                    store_cursor_X();
                    return;

                case Keys.Right:
                    if (!input_handler.shift) clear_selection();
                    if (input_handler.shift && !has_selection())
                        start_selection();

                    if (!input_handler.ctrl)
                        cursor_right(1);
                    else
                        cursor_right(find_word_size_right_of_cursor());

                    if (input_handler.shift)
                        end_selection();

                    store_cursor_X();
                    return;


                //spacing
                case Keys.Tab:
                    if (capture_tab) insert_text("    ");
                    return;

                case Keys.Enter:
                    if (multiline) insert_newline();
                    return;

                case Keys.Space:
                    insert_text(" ");
                    return;

                case Keys.Back:
                    if (!input_handler.ctrl)
                        backspace();
                    else
                        backspace_word();
                    return;

                case Keys.Delete:
                    if (!input_handler.ctrl)
                        delete();
                    else
                        delete_word();
                    return;

                case Keys.Insert:
                    if (!input_handler.shift) insert_mode = !insert_mode;
                    else paste();
                    return;

                //numbers
                case Keys.D0:
                    if (!input_handler.shift) insert_text("0");
                    else insert_text(")");
                    return;
                case Keys.D1:
                    if (!input_handler.shift) insert_text("1");
                    else insert_text("!");
                    return;
                case Keys.D2:
                    if (!input_handler.shift) insert_text("2");
                    else insert_text("@");
                    return;
                case Keys.D3:
                    if (!input_handler.shift) insert_text("3");
                    else insert_text("#");
                    return;
                case Keys.D4:
                    if (!input_handler.shift) insert_text("4");
                    else insert_text("$");
                    return;
                case Keys.D5:
                    if (!input_handler.shift) insert_text("5");
                    else insert_text("%");
                    return;
                case Keys.D6:
                    if (!input_handler.shift) insert_text("6");
                    else insert_text("^");
                    return;
                case Keys.D7:
                    if (!input_handler.shift) insert_text("7");
                    else insert_text("&");
                    return;
                case Keys.D8:
                    if (!input_handler.shift) insert_text("8");
                    else insert_text("*");
                    return;
                case Keys.D9:
                    if (!input_handler.shift) insert_text("9");
                    else insert_text("(");
                    return;

                case Keys.NumPad0:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("0");
                        else insert_mode = !insert_mode;
                    } else {
                        if (!input_handler.shift) insert_mode = !insert_mode;
                        else paste();
                    }
                    return;

                case Keys.NumPad1:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("1");
                        else cursor_end_line();
                    } else {
                        if (!input_handler.shift) cursor_end_line();
                        else cursor_end_line(); //also select
                    }
                    return;

                case Keys.NumPad2:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("2");
                        else cursor_down();
                    } else {
                        if (!input_handler.shift) cursor_down();
                        else cursor_down(); //also select
                    }
                    return;

                case Keys.NumPad3:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("3");
                        else page_down();
                    } else {
                        if (!input_handler.shift) page_down();
                        else page_down(); //also select
                    }
                    return;

                case Keys.NumPad4:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("4");
                        else cursor_left();
                    } else {
                        if (!input_handler.shift) cursor_left();
                        else cursor_left(); //also select
                    }
                    return;

                case Keys.NumPad5:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("5");
                        else return;
                    } else {
                        return;
                    }
                    return;

                case Keys.NumPad6:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("6");
                        else cursor_right();
                    } else {
                        if (!input_handler.shift) cursor_right();
                        else cursor_right(); //also select
                    }
                    return;

                case Keys.NumPad7:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("7");
                        else cursor_home_line();
                    } else {
                        if (!input_handler.shift) cursor_home_line();
                        else cursor_home_line(); //also select
                    }
                    return;

                case Keys.NumPad8:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("8");
                        else cursor_up();
                    } else {
                        if (!input_handler.shift) cursor_up();
                        else cursor_up(); //also select
                    }
                    return;

                case Keys.NumPad9:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text("9");
                        else page_up();
                    } else {
                        if (!input_handler.shift) page_up();
                        else page_up(); //also select
                    }
                    return;

                case Keys.Multiply: insert_text("*"); return;
                case Keys.Add: insert_text("+"); return;
                case Keys.Separator: break;
                case Keys.Subtract: insert_text("-"); return;
                case Keys.Decimal:
                    if (Input.num_lock) {
                        if (!input_handler.shift) insert_text(".");
                        else delete_text_at_cursor(1);
                    } else {
                        delete_text_at_cursor(1);
                    }
                    return;
                case Keys.Divide: insert_text("/"); return;

                //A-Z
                case Keys.A:
                    if (input_handler.ctrl) {
                        select_all();

                    } else { 
                            if (!input_handler.shift) insert_text("a");
                            else insert_text("A");
                    }
                    return;

                case Keys.B:
                    if (!input_handler.shift) insert_text("b");
                    else insert_text("B");
                    return;

                case Keys.C:
                    if (input_handler.ctrl) {
                        copy();

                    } else {
                        if (!input_handler.shift) insert_text("c");
                        else insert_text("C");
                    }

                    return;

                case Keys.D:
                    if (input_handler.ctrl) {
                        duplicate_line();
                    } else {
                        if (!input_handler.shift) insert_text("d");
                        else insert_text("D");
                    }

                    return;


                case Keys.E:
                    if (!input_handler.shift) insert_text("e");
                    else insert_text("E");
                    return;
                case Keys.F:
                    if (!input_handler.shift) insert_text("f");
                    else insert_text("F");
                    return;
                case Keys.G:
                    if (!input_handler.shift) insert_text("g");
                    else insert_text("G");
                    return;
                case Keys.H:
                    if (!input_handler.shift) insert_text("h");
                    else insert_text("H");
                    return;
                case Keys.I:
                    if (!input_handler.shift) insert_text("i");
                    else insert_text("I");
                    return;
                case Keys.J:
                    if (!input_handler.shift) insert_text("j");
                    else insert_text("J");
                    return;
                case Keys.K:
                    if (!input_handler.shift) insert_text("k");
                    else insert_text("K");
                    return;
                case Keys.L:
                    if (!input_handler.shift) insert_text("l");
                    else insert_text("L");
                    return;
                case Keys.M:
                    if (!input_handler.shift) insert_text("m");
                    else insert_text("M");
                    return;
                case Keys.N:
                    if (!input_handler.shift) insert_text("n");
                    else insert_text("N");
                    return;
                case Keys.O:
                    if (!input_handler.shift) insert_text("o");
                    else insert_text("O");
                    return;
                case Keys.P:
                    if (!input_handler.shift) insert_text("p");
                    else insert_text("P");
                    return;
                case Keys.Q:
                    if (!input_handler.shift) insert_text("q");
                    else insert_text("Q");
                    return;
                case Keys.R:
                    if (!input_handler.shift) insert_text("r");
                    else insert_text("R");
                    return;
                case Keys.S:
                    if (!input_handler.shift) insert_text("s");
                    else insert_text("S");
                    return;
                case Keys.T:
                    if (!input_handler.shift) insert_text("t");
                    else insert_text("T");
                    return;
                case Keys.U:
                    if (!input_handler.shift) insert_text("u");
                    else insert_text("U");
                    return;

                case Keys.V:
                    if (input_handler.ctrl) {
                        paste();

                    } else {
                        if (!input_handler.shift) insert_text("v");
                        else insert_text("V");
                    }

                    return;

                case Keys.W:
                    if (!input_handler.shift) insert_text("w");
                    else insert_text("W");
                    return;

                case Keys.X:
                    if (input_handler.ctrl) {
                        cut();
                    
                    } else {
                        if (!input_handler.shift) insert_text("x");
                        else insert_text("X");
                    }

                    return;

                case Keys.Y:
                    if (!input_handler.shift) insert_text("y");
                    else insert_text("Y");
                    return;
                case Keys.Z:
                    if (!input_handler.shift) insert_text("z");
                    else insert_text("Z");
                    return;

                case Keys.OemSemicolon:
                    if (!input_handler.shift) insert_text(";");
                    else insert_text(":");
                    return;
                case Keys.OemPlus:
                    if (!input_handler.shift) insert_text("=");
                    else insert_text("+");
                    return;
                case Keys.OemComma:
                    if (!input_handler.shift) insert_text(",");
                    else insert_text("<");
                    return;
                case Keys.OemMinus:
                    if (!input_handler.shift) insert_text("-");
                    else insert_text("_");
                    return;
                case Keys.OemPeriod:
                    if (!input_handler.shift) insert_text(".");
                    else insert_text(">");
                    return;
                case Keys.OemQuestion:
                    if (!input_handler.shift) insert_text("/");
                    else insert_text("?");
                    return;
                case Keys.OemTilde:
                    if (!input_handler.shift) insert_text("`");
                    else insert_text("~");
                    return;
                case Keys.OemOpenBrackets:
                    if (!input_handler.shift) insert_text("[");
                    else insert_text("{");
                    return;
                case Keys.OemPipe:
                    if (!input_handler.shift) insert_text("\\");
                    else insert_text("|");
                    return;
                case Keys.OemCloseBrackets:
                    if (!input_handler.shift) insert_text("]");
                    else insert_text("}");
                    return;
                case Keys.OemQuotes:
                    if (!input_handler.shift) insert_text("'");
                    else insert_text("\"");
                    return;
                case Keys.OemBackslash: break;

                case Keys.F1: break;
                case Keys.F2: break;
                case Keys.F3: break;
                case Keys.F4: break;
                case Keys.F5: break;
                case Keys.F6: break;
                case Keys.F7: break;
                case Keys.F8: break;
                case Keys.F9: break;
                case Keys.F10: break;
                case Keys.F11: break;
                case Keys.F12: break;
                case Keys.F13: break;
                case Keys.F14: break;
                case Keys.F15: break;
                case Keys.F16: break;
                case Keys.F17: break;
                case Keys.F18: break;
                case Keys.F19: break;
                case Keys.F20: break;
                case Keys.F21: break;
                case Keys.F22: break;
                case Keys.F23: break;
                case Keys.F24: break;

                case Keys.CapsLock: break;
                case Keys.NumLock: break;
                case Keys.Scroll: break;

                case Keys.LeftShift: break;
                case Keys.RightShift: break;
                case Keys.LeftControl: break;
                case Keys.RightControl: break;
                case Keys.LeftAlt: break;
                case Keys.RightAlt: break;
            }
        }

    }
}
