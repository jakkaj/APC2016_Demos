namespace ImageSearch.Contract
{
    public interface IImageTools
    {
        byte[] MaxResizeImage(byte[] sourceImage, float maxWidth, float maxHeight);
    }
}