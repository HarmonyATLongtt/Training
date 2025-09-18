using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace TreeViewProject.Behaviors
{
    public class MediaElementLoopBehavior : Behavior<MediaElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            // Chỉ cần Manual để mình chủ động gọi Play()
            AssociatedObject.LoadedBehavior = MediaState.Manual;
            AssociatedObject.UnloadedBehavior = MediaState.Stop;

            AssociatedObject.Loaded += (s, e) =>
            {
                AssociatedObject.Position = TimeSpan.Zero;
                AssociatedObject.Play();
            };

            AssociatedObject.MediaOpened += MediaOpened;
            AssociatedObject.MediaEnded += MediaEnded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MediaOpened -= MediaOpened;
            AssociatedObject.MediaEnded -= MediaEnded;
            base.OnDetaching();
        }

        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            // Reset về đầu và play
            AssociatedObject.Position = TimeSpan.Zero;
            AssociatedObject.Play();
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            // Loop lại
            AssociatedObject.Position = TimeSpan.Zero;
            AssociatedObject.Play();
        }
    }
}