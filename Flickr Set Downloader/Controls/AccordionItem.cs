using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlickrSetDownloader.Controls
{
    /// <summary>
    /// AccordionItem
    /// </summary>
    public class AccordionItem : HeaderedContentControl
    {
        #region IsExpanded

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(AccordionItem), new PropertyMetadata(false, new PropertyChangedCallback(OnIsExpandedChanged)));

        private static void OnIsExpandedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            AccordionItem item = sender as AccordionItem;
            if (item != null)
            {
                item.OnIsExpandedChanged(e);
            }
        }

        protected virtual void OnIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;

            if (newValue)
            {
                this.OnExpanded();
            }
            else
            {
                this.OnCollapsed();
            }
        } 

        #endregion

        #region Expand Events

        /// <summary>
        /// Raised when selected
        /// </summary>
        public event RoutedEventHandler Expanded
        {
            add { AddHandler(ExpandedEvent, value); }
            remove { RemoveHandler(ExpandedEvent, value); }
        }

        public static RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent(
            "Expanded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AccordionItem));

        /// <summary>
        /// Raised when unselected
        /// </summary>
        public event RoutedEventHandler Collapsed
        {
            add { AddHandler(CollapsedEvent, value); }
            remove { RemoveHandler(CollapsedEvent, value); }
        }

        public static RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent(
            "Collapsed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AccordionItem));

        protected virtual void OnExpanded()
        {
            Accordion parentAccordion = this.ParentAccordion;
            if (parentAccordion != null)
            {
                parentAccordion.ExpandedItem = this;
            }
            RaiseEvent(new RoutedEventArgs(ExpandedEvent, this));
        }

        protected virtual void OnCollapsed()
        {
            RaiseEvent(new RoutedEventArgs(CollapsedEvent, this));            
        }

        #endregion

        #region ExpandCommand

        public static RoutedCommand ExpandCommand = new RoutedCommand("Expand", typeof(AccordionItem));

        private static void OnExecuteExpand(object sender, ExecutedRoutedEventArgs e)
        {
            AccordionItem item = sender as AccordionItem;
            if (!item.IsExpanded)
            {
                item.IsExpanded = true;
            }
        }

        private static void CanExecuteExpand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sender is AccordionItem;
        }

        #endregion

        #region ParentAccordion

        private Accordion ParentAccordion
        {
            get { return ItemsControl.ItemsControlFromItemContainer(this) as Accordion; }
        } 

        #endregion

        #region Constructor

        static AccordionItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AccordionItem), new FrameworkPropertyMetadata(typeof(AccordionItem)));

            CommandBinding expandCommandBinding = new CommandBinding(ExpandCommand, OnExecuteExpand, CanExecuteExpand);
            CommandManager.RegisterClassCommandBinding(typeof(AccordionItem), expandCommandBinding);
        }    

        #endregion     
    }
}
