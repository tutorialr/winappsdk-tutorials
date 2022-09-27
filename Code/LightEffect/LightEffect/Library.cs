using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
internal class Library
{
    private PointLight _light;
    public void SetLight(FrameworkElement element)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;
        _light = compositor.CreatePointLight();
        _light.Color = Colors.White;
        _light.CoordinateSpace = visual;
        _light.Targets.Add(visual);
        _light.Offset = new System.Numerics.Vector3(
        -(float)element.ActualWidth * 2,
        (float)element.ActualHeight / 2,
        (float)element.ActualHeight);
        var animation = compositor.CreateScalarKeyFrameAnimation();
        animation.IterationBehavior = AnimationIterationBehavior.Forever;
        animation.InsertKeyFrame(1, 2 * (float)element.ActualWidth);
        animation.Duration = TimeSpan.FromSeconds(5.0f);
        _light.StartAnimation("Offset.X", animation);
    }
    public void ClearLight()
    {
        if (_light != null)
            _light.Targets.RemoveAll();
    }
}
