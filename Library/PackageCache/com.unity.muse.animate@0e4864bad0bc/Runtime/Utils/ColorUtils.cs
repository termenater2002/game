using UnityEngine;

namespace Unity.Muse.Animate
{
    static class ColorUtils
    {
        public static Color GetColorOverlay(Color a, Color b)
        {
            return new Color(
                GetChannelOverlay(a.r, b.r),
                GetChannelOverlay(a.g, b.g),
                GetChannelOverlay(a.b, b.b),
                GetChannelOverlay(a.a, b.a));
        }

        public static float GetChannelOverlay(float a, float b)
        {
            // Multiply
            if (a < 0.5f)
            {
                return a * b;
            }

            // Screen
            return 1 - (1 - a) * (1 - b);
        }

        
        public static Color MergeBlend(Color baseColor, Color blendColor, float opacity)
        {
            var r = Mathf.Max(baseColor.r + blendColor.r);
            var g = Mathf.Max(baseColor.g + blendColor.g);
            var b = Mathf.Max(baseColor.b + blendColor.b);
            
            // Assuming baseColor's alpha channel is used for the result
            var a = baseColor.a;

            // Preserve the backgrounds
            var threshold = 0.005f;

            if (Mathf.Abs(baseColor.r - blendColor.r) < threshold)
            {
                r = baseColor.r;
            }

            if (Mathf.Abs(baseColor.g - blendColor.g) < threshold)
            {
                g = baseColor.g;
            }

            if (Mathf.Abs(baseColor.b - blendColor.b) < threshold)
            {
                b = baseColor.b;
            }

            // Lerp between the base color and the additive blended color using the opacity value
            r = Mathf.Lerp(baseColor.r, r, opacity);
            g = Mathf.Lerp(baseColor.g, g, opacity);
            b = Mathf.Lerp(baseColor.b, b, opacity);

            return new Color(r, g, b, a);
        }

        public static Color AdditiveBlend(Color baseColor, Color blendColor, float opacity)
        {
            var r = baseColor.r + blendColor.r;
            var g = baseColor.g + blendColor.g;
            var b = baseColor.b + blendColor.b;
            
            // Assuming baseColor's alpha channel is used for the result
            var a = baseColor.a; 

            // Lerp between the base color and the additive blended color using the opacity value
            r = Mathf.Lerp(baseColor.r, r, opacity);
            g = Mathf.Lerp(baseColor.g, g, opacity);
            b = Mathf.Lerp(baseColor.b, b, opacity);

            return new Color(r, g, b, a);
        }

        public static Color OverlayBlend(Color baseColor, Color blendColor, float opacity)
        {
            var r = (baseColor.r < 0.5f) ? (2 * baseColor.r * blendColor.r) : (1 - 2 * (1 - baseColor.r) * (1 - blendColor.r));
            var g = (baseColor.g < 0.5f) ? (2 * baseColor.g * blendColor.g) : (1 - 2 * (1 - baseColor.g) * (1 - blendColor.g));
            var b = (baseColor.b < 0.5f) ? (2 * baseColor.b * blendColor.b) : (1 - 2 * (1 - baseColor.b) * (1 - blendColor.b));
            
            // Assuming baseColor's alpha channel is used for the result
            var a = baseColor.a;

            // Lerp between the base color and the overlay blended color using the opacity value
            r = Mathf.Lerp(baseColor.r, r, opacity);
            g = Mathf.Lerp(baseColor.g, g, opacity);
            b = Mathf.Lerp(baseColor.b, b, opacity);

            return new Color(r, g, b, a);
        }

        public static Color OverlayBlend(Color baseColor, Color blendColor)
        {
            var r = (baseColor.r < 0.5f) ? (2 * baseColor.r * blendColor.r) : (1 - 2 * (1 - baseColor.r) * (1 - blendColor.r));
            var g = (baseColor.g < 0.5f) ? (2 * baseColor.g * blendColor.g) : (1 - 2 * (1 - baseColor.g) * (1 - blendColor.g));
            var b = (baseColor.b < 0.5f) ? (2 * baseColor.b * blendColor.b) : (1 - 2 * (1 - baseColor.b) * (1 - blendColor.b));
            
            // Assuming baseColor's alpha channel is used for the result
            var a = baseColor.a;

            return new Color(r, g, b, a);
        }
        
        public static Color MultiplyRGB(Color color, float amount)
        {
            color.r *= amount;
            color.g *= amount;
            color.b *= amount;
            color.r = Mathf.Clamp01(color.r);
            color.g = Mathf.Clamp01(color.g);
            color.b = Mathf.Clamp01(color.b);
            return color;
        }

        public static Color MultiplyAlpha(Color color, float amount)
        {
            color.a *= amount;
            color.a = Mathf.Clamp01(color.a);
            return color;
        }

        public static Color AddRGB(Color color, float amount)
        {
            color.r += amount;
            color.g += amount;
            color.b += amount;
            color.r = Mathf.Clamp01(color.r);
            color.g = Mathf.Clamp01(color.g);
            color.b = Mathf.Clamp01(color.b);
            return color;
        }

        public static Color AddAlpha(Color color, float amount)
        {
            color.a += amount;
            color.a = Mathf.Clamp01(color.a);
            return color;
        }
    }
}
