## Swoop

A set of UI elements and systems to manage them, for creating user interfaces. Allows for building things like debug overlays, editor UIs, or even entire applications (a questionable idea). Absolutely not immediate mode. Hella state. 

Everything is implemented as small ProFont and extreme minimalist 1px bright-lines-on-dark-backgrounds, because I first used bbLean at a formative age. The goal is to eventually make it possible to globally 'swap out' important parts of different elements- for example, assigning textures for a button's standard, mouseover, and clicked states, to effectively "skin" all buttons belonging to a UIElementManager. Until at least most elements are implemented and working though, 20/20 vision or better only.

This project will not work as-is and requires MGRawInputLib from [MGRawInput](https://github.com/stencho/MGRawInput) (for handling input and additional things, such as window dragging and threaded input handling)

#### Implemented UI Elements
- [x] Button
- [x] Check Box
- [ ] Combo Box
- [ ] Cursor
- [x] Dialog Box
- [x] Label
- [ ] List Box
- [ ] Menu Strip
- [ ] Numeric Up/Down
- [ ] Multi-choice Option Slider
- [x] Panel
- [ ] Progress Bar (+ combined TrackBar)
- [ ] Radio Buttons
- [ ] Resize Handle
- [ ] Tab Control
- [ ] Text Box
- [x] Title Bar (with window dragging)
- [x] Toggle Button


#### Most current SwoopDemo screenshot
![Most current major change screenshot](current.png)


#### Usage:
```csharp
using SwoopLib;

internal static Point resolution = new Point(800, 600);

void Initialize() {
  Swoop.Initialize(this, resolution);
}

void LoadContent() {
  Swoop.Load(GraphicsDevice, graphics, Content, resolution);
  build_UI();
}

void build_UI() {
  //add a button
  Swoop.UI.add_element(new Button("demo_button", "demo", Vector2.One * 10f));

  ((Button)Swoop.UI.elements["demo_button"]).click_action = () => {
    //do something when the button is clicked
  };
}

void Update() {
  Swoop.Update();
}

void Draw() {  
  //Swoop.Draw() draws to Swoop.render_target_output, so that in turn needs 
  //to be drawn to the screen or wherever it's needed
  //If Swoop.fill_background is set to true, the background will be cleared 
  //to Swoop.UI_background_color, otherwise it'll be transparent
  Swoop.Draw();
  GraphicsDevice.SetRenderTarget(null);
  Drawing.image(Swoop.render_target_output, Vector2.Zero, resolution);
}
```

#### Creating elements:
```csharp
public class demo_element : UIElement {
    public demo_element(string name, Vector2 position, Vector2 size) : base(name, position, size) {
        //if this is enabled, the element will first draw everything in draw_rt
        //to this.draw_target, then run draw() later
        enable_render_target = true;
        //can be targeted by 
        can_be_focused = true;
        //can still be interacted with when a dialog object is set
        ignore_dialog = false;
    }
    internal override void added() { 
      //occurs after element has had its parent set and has been added to a 
      //UIElementManager's elements array
    }
    internal override void update() { /*update*/ }        
    internal override void draw_rt() {
      //fill element with color
      Drawing.fill_rect(Vector2.Zero, size, Swoop.get_color(this));
    }
    internal override void draw() {          
      //draw the rendertarget to the output
      Drawing.image(draw_target, Vector2.Zero, size);
    }
}
```