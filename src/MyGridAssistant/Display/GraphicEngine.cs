using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace MyGridAssistant
{
    public class GraphicEngine
    {
        private readonly float _ratio;
        private readonly List<MySprite> _sprites = new List<MySprite>();
        private readonly IMyTextSurface _textSurface;
        private readonly float _yOffset;

        public GraphicEngine(IMyTextSurface textSurface, bool screenSizeRatio)
        {
            _textSurface = textSurface;
            _ratio = screenSizeRatio
                ? textSurface.SurfaceSize.Y / textSurface.SurfaceSize.X
                : 1f;

            _yOffset = Math.Abs(textSurface.SurfaceSize.Y / textSurface.SurfaceSize.X - 1f) > 0
                ? (textSurface.SurfaceSize.X - textSurface.SurfaceSize.Y) / 2
                : 0;

            _textSurface.ContentType = ContentType.SCRIPT;
            _textSurface.Script = "None";
        }

        public Color? BackgroundColor { get; set; }

        public void Draw()
        {
            using (var frame = _textSurface.DrawFrame())
            {
                if (BackgroundColor != null)
                {
                    var sprite = new MySprite(
                        SpriteType.TEXTURE,
                        "SquareSimple",
                        size: new Vector2(512.0f, 512.0f * _ratio),
                        color: new Color(0, 0, 0, 255));

                    frame.Add(sprite);
                }

                _sprites.ForEach(sprite => frame.Add(sprite));
            }
        }

        public float AddText(
            string text,
            float scale,
            float left,
            float top,
            TextAlignment alignment,
            Color color)
        {
            const float Y_MARGIN = -5f;
            const float TEXT_BOX_SIZE = 20f;

            var sprite = MySprite.CreateText(
                text,
                "debug",
                color,
                scale * _ratio,
                alignment);

            sprite.Position = new Vector2(
                left,
                top + _yOffset + Y_MARGIN * _ratio);

            _sprites.Add(sprite);

            return top + TEXT_BOX_SIZE * (scale * _ratio) + 1f;
        }

        public float AddSprite(
            string spriteId,
            float width,
            float height,
            Color color,
            float left,
            float top,
            TextAlignment alignment,
            float rotation,
            bool preserveRatio)
        {
            var sizeX = width *
                        (preserveRatio
                            ? _ratio
                            : 1);

            var sizeY = height * _ratio;

            var posY = top + _yOffset;

            var sprite = new MySprite(
                SpriteType.TEXTURE,
                spriteId,
                new Vector2(left, posY + sizeY / 2),
                new Vector2(sizeX, sizeY),
                color,
                null,
                alignment,
                rotation
            );

            _sprites.Add(sprite);

            return top + sizeY + 1f;
        }

        public float AddProgressBar(
            string text,
            float width,
            float barHeight,
            float textScale,
            float left,
            float top,
            double fillRatio,
            Color? color = null)
        {
            if (color == null)
                color = _textSurface.ScriptBackgroundColor;

            var textLeftBegin = left + 30f;
            var margin = 5f * textScale;

            var barTop = AddText(
                text,
                textScale,
                textLeftBegin,
                top,
                TextAlignment.LEFT,
                _textSurface.ScriptForegroundColor
            );

            var barEnd = AddSprite(
                "SquareSimple",
                width,
                barHeight,
                new Color(10, 10, 10, 200),
                left,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false
            );

            AddSprite(
                "SquareSimple",
                Convert.ToSingle(fillRatio) * width,
                barHeight,
                color.Value,
                left,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false
            );

            AddSprite(
                "SquareSimple",
                3f,
                barHeight,
                new Color(0, 0, 0, 255),
                left + width / 4,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false
            );

            AddSprite(
                "SquareSimple",
                3f,
                barHeight,
                new Color(0, 0, 0, 255),
                left + width / 4 * 2,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false
            );

            AddSprite(
                "SquareSimple",
                3f,
                barHeight,
                new Color(0, 0, 0, 255),
                left + width / 4 * 3,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false
            );

            AddText(
                (fillRatio * 100).ToString("F2") + "%",
                textScale * .75f,
                width + left,
                7f * textScale + top,
                TextAlignment.RIGHT,
                _textSurface.ScriptBackgroundColor);

            return barEnd + 1;
        }
    }
}
