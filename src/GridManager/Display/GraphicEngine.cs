using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
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

        public void AddText(
            string text,
            float scale,
            float leftPadding,
            float topPadding,
            TextAlignment alignment,
            Color color)
        {
            var sprite = MySprite.CreateText(
                text,
                "debug",
                color,
                scale * _ratio,
                alignment);

            sprite.Position = new Vector2(
                leftPadding,
                topPadding * _ratio + _yOffset);

            _sprites.Add(sprite);
        }

        public void AddSprite(
            string spriteId,
            float width,
            float height,
            Color color,
            float leftPadding,
            float topPadding,
            TextAlignment alignment,
            float rotation,
            bool preserveRatio)
        {
            var sprite = new MySprite(
                SpriteType.TEXTURE,
                spriteId,
                size: new Vector2(
                    width *
                    (preserveRatio
                        ? _ratio
                        : 1),
                    height * _ratio),
                alignment: alignment,
                color: color)
            {
                Position = new Vector2(
                    leftPadding,
                    topPadding * _ratio + _yOffset),
                RotationOrScale = rotation
            };

            _sprites.Add(sprite);
        }
    }
}
