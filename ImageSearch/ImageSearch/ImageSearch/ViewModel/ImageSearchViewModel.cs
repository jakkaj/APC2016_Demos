using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmHelpers;
using ImageSearch.Services;
using System.Net.Http;
using Newtonsoft.Json;
using ImageSearch.Model;
using Acr.UserDialogs;
using ImageSearch.Model.BingSearch;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System.IO;
using ImageSearch.Contract;
using Plugin.Media;
using Plugin.Media.Abstractions;

namespace ImageSearch.ViewModel
{
    public class ImageSearchViewModel
    {
        private readonly IImageTools _imageTools;
        public ObservableRangeCollection<ImageResult> Images { get; }

        public ImageSearchViewModel(IImageTools imageTools)
        {
            _imageTools = imageTools;
            Images = new ObservableRangeCollection<ImageResult>();
        }
        
        public async Task<bool> SearchForImagesAsync(string query)
        {
			//Bing Image API
			var url = $"https://api.cognitive.microsoft.com/bing/v5.0/images/" + 
				      $"search?q={query}" +
					  $"&count=20&offset=0&mkt=en-us&safeSearch=Strict";

            var requestHeaderKey = "Ocp-Apim-Subscription-Key";
            var requestHeaderValue = CognitiveServicesKeys.BingSearch;
            try
			{
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add(requestHeaderKey, requestHeaderValue);

                    var json = await client.GetStringAsync(url);

                    var result = JsonConvert.DeserializeObject<SearchResult>(json);

                    Images.ReplaceRange(result.Images.Select(i => new ImageResult
                    {
                        ContextLink = i.HostPageUrl,
                        FileFormat = i.EncodingFormat,
                        ImageLink = i.ContentUrl,
                        ThumbnailLink = i.ThumbnailUrl,
                        Title = i.Name
                    }));
                }
			}
			catch (Exception ex)
			{	
				await UserDialogs.Instance.AlertAsync("Unable to query images: " + ex.Message);
				return false;
			}
			return true;
        }

        public async Task<string> GetImageDescription(Stream imageStream)
        {
            try
            {
                VisionServiceClient visionClient = new VisionServiceClient(CognitiveServicesKeys.VisionAPI);
                VisualFeature[] features = { VisualFeature.Tags, VisualFeature.Categories, VisualFeature.Description, VisualFeature.Faces };
                var result = await visionClient.AnalyzeImageAsync(imageStream, features.ToList(), null);
                return InterpretAnalysisResult(result);
            }
            catch (Exception ex)
            {
                return "Unable to Analyze Image " + ex.Message;
            }
        }

        public async Task<string> GetImageDescription(string imageUrl)
        {
            try
            {
                VisionServiceClient visionClient = new VisionServiceClient(CognitiveServicesKeys.VisionAPI);
                VisualFeature[] features = { VisualFeature.Tags, VisualFeature.Categories, VisualFeature.Description, VisualFeature.Faces };
                var result =  await visionClient.AnalyzeImageAsync(imageUrl, features.ToList(), null);
                return InterpretAnalysisResult(result);
            }   
            catch (Exception ex)
            {
                return "Unable to Analyze Image " + ex.Message;
            }       
        }

        private string InterpretAnalysisResult(AnalysisResult analysisResult)
        {
            try
            {
                var caption = analysisResult?.Description?.Captions.FirstOrDefault();
                if (caption != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(caption.Text);
                    sb.AppendLine();
                    sb.AppendLine($"Confidence - {caption.Confidence:P}");
                    sb.AppendLine();

                    // get tags
                    var tags = analysisResult?.Tags.Select(i => i.Name);
                    sb.AppendLine("Tags - " + string.Join(", ", tags));

                    // get faces
                    var faces = analysisResult?.Faces.Select(i => i.Gender + " " + i.Age);
                    if (faces.Count() > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine("Faces - " + string.Join(", ", faces));
                    }
                    return sb.ToString();
                }
                return "Couldn't work out the caption";
            }
            catch (Exception ex)
            {
                return $"Problem analysing image - {ex.Message}";
            }
        }


        public async Task TakePhotAsync(bool UseCamera = true)
        {
            MediaFile file = null;
            await CrossMedia.Current.Initialize();
            

            if (UseCamera)
            {
                file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "Samples",
                    Name = "test.jpg",
                    SaveToAlbum = false
                    
                });
            }
            else
            {
                file = await CrossMedia.Current.PickPhotoAsync();
            }

            if (file == null)
                await UserDialogs.Instance.AlertAsync("No Photo Taken", "Analysis Result");
            else
            {
                using (var stream = file.GetStream())
                {
                    byte[] bytes = new byte[stream.Length];
                    await stream.ReadAsync(bytes, 0, bytes.Length);
                    var adjusted = _imageTools.MaxResizeImage(bytes, 1024, 768);
                    using (var ms = new MemoryStream(adjusted))
                    {
                        var description = await GetImageDescription(ms);
                        await UserDialogs.Instance.AlertAsync(description, "Analysis Result");
                    }
                }
            }       
        }
    }
}
