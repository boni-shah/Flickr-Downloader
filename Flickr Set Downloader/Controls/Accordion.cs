using System.Windows;
using System.Windows.Controls;

namespace FlickrSetDownloader.Controls
{
    /// <summary>
    /// Accordion
    /// </summary>
    public class Accordion : ItemsControl
    {
        #region ExpandedItem

        /// <summary>
        /// Gets/Sets which item to expand
        /// </summary>
        public object ExpandedItem
        {
            get { return (object)GetValue(ExpandedItemProperty); }
            set { SetValue(ExpandedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandedItemProperty = DependencyProperty.Register(
            "ExpandedItem", typeof(object), typeof(Accordion),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnExpandedItemChanged)));

        private static void OnExpandedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Accordion shelf = sender as Accordion;
            if (shelf != null)
            {
                shelf.OnExpandedItemChanged(e.OldValue, e.NewValue);
            }
        }

        protected virtual void OnExpandedItemChanged(object oldValue, object newValue)
        {
            AccordionItem oldItem = this.ItemContainerGenerator.ContainerFromItem(oldValue) as AccordionItem;

            if (oldItem != null)
            {
                oldItem.IsExpanded = false;
            }
        }

        #endregion

        #region Constructors

        static Accordion()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Accordion), new FrameworkPropertyMetadata(typeof(Accordion)));
        } 

        #endregion

        #region Overrides

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new AccordionItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is AccordionItem;
        }

        #endregion
    }
}
