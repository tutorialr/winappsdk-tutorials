using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Shapes;
internal class Library
{
    private SpriteVisual _shade;
    public void SetShade(Shape shape, FrameworkElement element)
    {
        var compositor = ElementCompositionPreview
        .GetElementVisual(shape).Compositor;
        _shade = compositor.CreateSpriteVisual();
        _shade.Size = new System.Numerics.Vector2(
        (float)shape.ActualWidth,
        (float)shape.ActualHeight);
        DropShadow shadow = compositor.CreateDropShadow();
        shadow.Color = Colors.Black;
        shadow.Offset = new System.Numerics.Vector3(10, 10, 0);
        shadow.Mask = shape.GetAlphaMask();
        _shade.Shadow = shadow;
        ElementCompositionPreview.SetElementChildVisual(element, _shade);
    }
    public void ClearShade()
    {
        if (_shade != null)
            _shade.Shadow = null;
    }
}
