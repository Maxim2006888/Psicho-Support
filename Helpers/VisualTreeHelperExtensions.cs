// Helpers/VisualTreeHelperExtensions.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Psicho_Support.Helpers
{
    public static class VisualTreeHelperExtensions
    {
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        public static T FindChild<T>(DependencyObject parent, string childName) where T : FrameworkElement
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element && element.Name == childName)
                    return element;

                var result = FindChild<T>(child, childName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}