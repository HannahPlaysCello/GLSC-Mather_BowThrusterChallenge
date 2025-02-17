using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BowThrust_MonoGame
{
    public class MenuManager
    {
        private SpriteFont _font;
        private int _selectedOption = 0;
        private bool _isKeyPressed = false;
        private string[] _options = { "Normal Mode", "Thruster Mode" };

        public MenuManager(SpriteFont font)
        {
            _font = font;
        }

        public int GetSelectedOption()
        {
            return _selectedOption;
        }

        public bool Update(KeyboardState keyboardState, Dictionary<string, Keys> controlKeyMap)
        {
            bool startGame = false;

            if (keyboardState.IsKeyDown(controlKeyMap["MenuUp"]) && !_isKeyPressed)
            {
                _selectedOption = 0;  // Normal Mode
                _isKeyPressed = true;
                Console.WriteLine("normal selected");
            }
            if (keyboardState.IsKeyDown(controlKeyMap["MenuDown"]) && !_isKeyPressed)
            {
                _selectedOption = 1;  // Thruster Mode
                _isKeyPressed = true;
                Console.WriteLine("thruster selected");
            }  

            if (keyboardState.IsKeyUp(controlKeyMap["MenuUp"]) && keyboardState.IsKeyUp(controlKeyMap["MenuDown"]))
            {
                _isKeyPressed = false;
            }

            if (keyboardState.IsKeyDown(controlKeyMap["Select"]))
            {
                startGame = true;
            }

            return startGame;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "Choose Boat Mode:", new Vector2(300, 200), Color.Red);
            for (int i = 0; i < _options.Length; i++)
            {
                Color color = (_selectedOption == i) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(_font, _options[i], new Vector2(300, 300 + (i * 50)), color);
            }
            spriteBatch.DrawString(_font, "Press ENTER to select", new Vector2(300, 450), Microsoft.Xna.Framework.Color.Gray);
        }
    }
}
