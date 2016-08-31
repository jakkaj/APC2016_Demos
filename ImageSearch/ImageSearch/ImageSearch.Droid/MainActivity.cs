using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.Widget;
using ImageSearch.Droid.Adapters;
using ImageSearch.ViewModel;
using Acr.UserDialogs;
using System.Threading.Tasks;

namespace ImageSearch.Droid
{
    [Activity(Label = "Image Search", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : BaseActivity
    {
        RecyclerView recyclerView;
        RecyclerView.LayoutManager layoutManager;
        ImageAdapter adapter;
        ProgressBar progressBar;

        ImageSearchViewModel viewModel;

        protected override int LayoutResource
        {
            get { return Resource.Layout.main; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            viewModel = new ImageSearchViewModel();

            //Setup RecyclerView
            adapter = new ImageAdapter(this, viewModel);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetAdapter(adapter);
            layoutManager = new GridLayoutManager(this, 2);
            recyclerView.SetLayoutManager(layoutManager);

            progressBar = FindViewById<ProgressBar>(Resource.Id.my_progress);
            progressBar.Visibility = ViewStates.Gone;

            var query = FindViewById<EditText>(Resource.Id.my_query);

            // Get our button from the layout resource,
            // and attach an event to it
            var clickButton = FindViewById<Button>(Resource.Id.my_button);
            clickButton.Click += async (sender, args) =>
            {
                clickButton.Enabled = false;
                progressBar.Visibility = ViewStates.Visible;

                await viewModel.SearchForImagesAsync(query.Text.Trim());

                progressBar.Visibility = ViewStates.Gone;
                clickButton.Enabled = true;
            };

            //Button Click event to get images
            adapter.ItemClick += async (object sender, ImageAdapterClickEventArgs e) =>
            {
                clickButton.Enabled = false;
                progressBar.Visibility = ViewStates.Visible;

                var result = await viewModel.GetImageDescription(viewModel.Images[e.Position].ImageLink);
                new AlertDialog.Builder(this)
                               .SetTitle("Image Analysis")
                               .SetMessage(result)
                               .SetNeutralButton("OK", delegate { })
                               .Show();

                progressBar.Visibility = ViewStates.Gone;
                clickButton.Enabled = true;
            };

            UserDialogs.Init(this);
            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(false);
        }

        // create toolbar menu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_photo, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_camera:
                    SelectImage(true);
                    break;
                case Resource.Id.action_pick:
                    SelectImage(false);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        private async Task SelectImage(bool UseCamera)
        {
            progressBar.Visibility = ViewStates.Visible;
            await viewModel.TakePhotAsync(UseCamera);
            progressBar.Visibility = ViewStates.Gone;
        }

    }
}

