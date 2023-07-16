using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SwoopDemo {
    internal class FPSCounter {
        public int frame_rate => _frame_rate;
        int _frame_rate;

        double _fps_timer = 0;
        long _frame_count = 0;

        public int update_frequency_ms { get; set; } = 100;        

        public void update(GameTime gt) {
            _fps_timer += gt.ElapsedGameTime.TotalMilliseconds;
            _frame_count++;

            if (_fps_timer >= update_frequency_ms) {
                _frame_rate = (int)(_frame_count * (1000.0/update_frequency_ms));
                _frame_count = 0;
                _fps_timer -= update_frequency_ms;
            }
        }
    }
}
