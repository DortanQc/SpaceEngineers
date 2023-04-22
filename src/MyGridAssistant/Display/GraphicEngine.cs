using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace MyGridAssistant
{
    public class GraphicEngine
    {
        private readonly Color _defaultTextColor = new Color(171, 146, 91, 255);
        private readonly IMyGridAssistantLogger _logger;
        private readonly float _ratio;
        private readonly List<MySprite> _sprites = new List<MySprite>();
        private readonly IMyTextSurface _textSurface;
        private readonly float _yOffset;

        public GraphicEngine(IMyTextSurface textSurface, bool screenSizeRatio, IMyGridAssistantLogger logger)
        {
            _textSurface = textSurface;

            _logger = logger;
            _ratio = screenSizeRatio
                ? textSurface.SurfaceSize.Y / textSurface.SurfaceSize.X
                : 1f;

            _yOffset = Math.Abs(textSurface.SurfaceSize.Y / textSurface.SurfaceSize.X - 1f) > 0
                ? (textSurface.SurfaceSize.X - textSurface.SurfaceSize.Y) / 2
                : 0;
        }

        public Color? BackgroundColor { get; set; }

        public void Draw()
        {
            using (var frame = _textSurface.DrawFrame())
            {
                var bgColor = BackgroundColor ?? _textSurface.ScriptBackgroundColor;
                {
                    var sprite = new MySprite(
                        SpriteType.TEXTURE,
                        "SquareSimple",
                        size: new Vector2(512.0f, 512.0f * _ratio),
                        color: bgColor);

                    frame.Add(sprite);
                }

                frame.AddRange(_sprites);
            }
        }

        public float AddText(
            string text,
            float scale,
            float left,
            float top,
            TextAlignment alignment,
            Color? color = null)
        {
            if (color == null)
                color = _defaultTextColor;

            const float Y_MARGIN = -5f;
            const float TEXT_BOX_SIZE = 20f;

            var sprite = MySprite.CreateText(
                text,
                "debug",
                color.Value,
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
            float left,
            float top,
            TextAlignment alignment,
            float rotation,
            bool preserveRatio,
            Color? color = null)
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
                color = new Color(61, 165, 244);

            var textLeftBegin = left + 30f;
            var margin = 5f * textScale;

            var barTop = AddText(
                text,
                textScale,
                textLeftBegin,
                top,
                TextAlignment.LEFT
            );

            var barEnd = AddSprite(
                "SquareSimple",
                width,
                barHeight,
                left,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false,
                new Color(47, 50, 52, 255)
            );

            AddSprite(
                "SquareSimple",
                Convert.ToSingle(fillRatio) * width,
                barHeight,
                left,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false,
                color.Value
            );

            AddSprite(
                "SquareSimple",
                3f,
                barHeight,
                left + width / 4,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false,
                new Color(0, 0, 0, 255)
            );

            AddSprite(
                "SquareSimple",
                3f,
                barHeight,
                left + width / 4 * 2,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false,
                new Color(0, 0, 0, 255)
            );

            AddSprite(
                "SquareSimple",
                3f,
                barHeight,
                left + width / 4 * 3,
                barTop + margin,
                TextAlignment.LEFT,
                0f,
                false,
                new Color(0, 0, 0, 255)
            );

            AddText(
                (fillRatio * 100).ToString("F2") + "%",
                textScale * .75f,
                width + left,
                7f * textScale + top,
                TextAlignment.RIGHT,
                color.Value);

            return barEnd + 1;
        }
    }
}
