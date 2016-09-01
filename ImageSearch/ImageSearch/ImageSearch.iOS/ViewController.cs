using System;
using Foundation;
using UIKit;
using ImageSearch.ViewModel;
using SDWebImage;
using System.Linq;
using Acr.UserDialogs;

namespace ImageSearch.iOS
{
    public partial class ViewController : UIViewController, IUICollectionViewDataSource, IUICollectionViewDelegate
    {
        ImageSearchViewModel viewModel;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            viewModel = new ImageSearchViewModel(new ImageTools());

            CollectionViewImages.WeakDataSource = this;
            CollectionViewImages.AllowsSelection = true;

            CollectionViewImages.Delegate = this;

            //Button Click event to get images
            ButtonSearch.TouchUpInside += async (sender, args) =>
            {
                ButtonSearch.Enabled = false;
                ActivityIsLoading.StartAnimating();

                await viewModel.SearchForImagesAsync(TextFieldQuery.Text);
                CollectionViewImages.ReloadData();

                ButtonSearch.Enabled = true;
                ActivityIsLoading.StopAnimating();
            };

            // toolbar buttons
            var cameraButton = new UIBarButtonItem(UIBarButtonSystemItem.Camera, 
            async (sender, e) =>
            {
                await viewModel.TakePhotAsync();
            });

            var pickButton = new UIBarButtonItem(UIBarButtonSystemItem.Organize,
            async (sender, e) =>
            {
                await viewModel.TakePhotAsync(false);
            });

            this.NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { cameraButton, pickButton };
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        public async void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            ActivityIsLoading.StartAnimating();

            string description = await viewModel.GetImageDescription(viewModel.Images[indexPath.Row].ImageLink);
            UIAlertView alert = new UIAlertView("Image Analysis",
                                                description, null, "OK", null);
            alert.Show();

            ActivityIsLoading.StopAnimating();
        }

        public nint GetItemsCount(UICollectionView collectionView, nint section) =>
            viewModel.Images.Count;

        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell("imagecell", indexPath) as ImageCell;

            var item = viewModel.Images[indexPath.Row];

            cell.Caption.Text = item.Title;

            cell.Image.SetImage(new NSUrl(item.ThumbnailLink));

            return cell;
        }


    }
}

