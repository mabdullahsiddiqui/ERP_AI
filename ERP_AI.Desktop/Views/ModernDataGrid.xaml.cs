using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ERP_AI.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ModernDataGrid.xaml
    /// </summary>
    public partial class ModernDataGrid : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(ModernDataGrid),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(object), typeof(ModernDataGrid),
                new PropertyMetadata(null));

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItems
        {
            get => GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        private ICollectionView _collectionView;

        public ModernDataGrid()
        {
            InitializeComponent();
            MainDataGrid.SelectionChanged += OnSelectionChanged;
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ModernDataGrid control)
            {
                control.UpdateItemsSource();
            }
        }

        private void UpdateItemsSource()
        {
            if (ItemsSource != null)
            {
                _collectionView = CollectionViewSource.GetDefaultView(ItemsSource);
                MainDataGrid.ItemsSource = _collectionView;
                
                if (_collectionView != null)
                {
                    _collectionView.Filter = FilterItems;
                    UpdateRowCount();
                }
            }
        }

        private bool FilterItems(object item)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
                return true;

            // Simple text search - can be enhanced for specific data types
            return item.ToString().ToLower().Contains(SearchBox.Text.ToLower());
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedCount();
        }

        private void UpdateRowCount()
        {
            if (_collectionView != null)
            {
                RowCountText.Text = _collectionView.Cast<object>().Count().ToString();
            }
        }

        private void UpdateSelectedCount()
        {
            SelectedCountText.Text = MainDataGrid.SelectedItems.Count.ToString();
        }

        // Public methods for external control
        public void Refresh()
        {
            _collectionView?.Refresh();
            UpdateRowCount();
        }

        public void ClearSelection()
        {
            MainDataGrid.UnselectAll();
        }

        public void SelectAll()
        {
            MainDataGrid.SelectAll();
        }

        public void ExportToCsv()
        {
            // Implementation for CSV export
            MessageBox.Show("CSV Export functionality will be implemented here.");
        }

        public void ExportToExcel()
        {
            // Implementation for Excel export
            MessageBox.Show("Excel Export functionality will be implemented here.");
        }
    }
}
