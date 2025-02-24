using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BowThrust_MonoGame
{
    public class MenuManager
    {
        private SpriteFont Font;
        private int _selectedOption = 0;
        private bool _isKeyPressed = false;
        private string[] _options = { "Practice: Normal Mode", "Practice: Thruster Mode", "Play the Navigation Challenge!" };

        public MenuManager(SpriteFont font)
        {
            Font = font;
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
                _selectedOption = (_selectedOption - 1 + _options.Length) % _options.Length;  //navigate up
                _isKeyPressed = true;
            }
            if (keyboardState.IsKeyDown(controlKeyMap["MenuDown"]) && !_isKeyPressed)
            {
                _selectedOption = (_selectedOption + 1) % _options.Length;  //navigate down
                _isKeyPressed = true;
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
            spriteBatch.DrawString(Font, "Choose Boat Mode:", new Vector2(300, 200), Color.Red);
            for (int i = 0; i < _options.Length; i++)
            {
                Color color = (_selectedOption == i) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(Font, _options[i], new Vector2(300, 300 + (i * 50)), color);
            }
            spriteBatch.DrawString(Font, "Press ENTER to select", new Vector2(300, 450), Microsoft.Xna.Framework.Color.Gray);
        }
    }
}
