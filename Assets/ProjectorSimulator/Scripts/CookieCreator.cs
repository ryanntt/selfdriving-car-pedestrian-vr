using UnityEngine;
using System.Linq;

public struct CookieData
{
    public float shift_v, shift_h, keystone_h, keystone_v, ratio, aspect;
    public CookieData(float shiftV, float shiftH, float keystoneH, float keystoneV, float throwRatio, float imageAspect)
    {
        shift_v = shiftV;
        shift_h = shiftH;
        keystone_h = keystoneH;
        keystone_v = keystoneV;
        ratio = throwRatio;
        aspect = imageAspect;
    }    
}

public struct ProjectedImageInCookieData
{
    public int imageLeftEdgeInCookie, imageTopEdgeInCookie, imageWidthInCookie, imageHeightInCookie, textureSize;
    public int imageCentreH, imageCentreV;
    public float keystoneMinWidth, keystoneMinHeight;
    public bool keystoneH_flip, keystoneV_flip, colour;
    public ProjectedImageInCookieData(int imgLeftEdge, int imgTopEdge, int imgWidth, int imgHeight, float keystone_minWidth, float keystone_minHeight, bool flipKeystoneH, bool flipKeystoneV, bool isColour, int size)
    {
        imageLeftEdgeInCookie = imgLeftEdge;
        imageTopEdgeInCookie = imgTopEdge;
        imageWidthInCookie = imgWidth;
        imageHeightInCookie = imgHeight;
        imageCentreH = imageLeftEdgeInCookie + imageWidthInCookie / 2;
        imageCentreV = imageTopEdgeInCookie + imageHeightInCookie / 2;
        keystoneMinWidth = keystone_minWidth;
        keystoneMinHeight = keystone_minHeight;
        keystoneH_flip = flipKeystoneH;
        keystoneV_flip = flipKeystoneV;
        colour = isColour;
        textureSize = size;
    }
}

public class Cookie
{
    public enum ImageType { Colour, Grey};
    Texture2D projectedImage = null;
    Color32[] imageColours = null;
    CookieData data;
    float imageWidth, imageHeight;
    float maxImageEdgeDistance;
    float metersToPixels = 1.0f;
    ImageType imageType = ImageType.Grey;
    Texture2D redCookie, greenCookie, blueCookie;
    Color32[] redColors, greenColors, blueColors;

    // used for calculating angle
    const float distance = 10.0f; // throw distance
    int textureSize = 1024; // texture width and height

    Light redLight, greenLight, blueLight;

    // data about where the actual image is located in the cookie (same for all channels)
    ProjectedImageInCookieData projectedImageData;

    public Cookie(CookieData cookieData, int cookieSize, Light red, Light green, Light blue, Texture2D imageToProject = null, bool colour = false)
    {
        textureSize = cookieSize;
        projectedImage = imageToProject;
        if (projectedImage != null)
            imageColours = projectedImage.GetPixels32();
        else
            imageColours = null;

        CreateTexture();

        if (colour)
            imageType = ImageType.Colour;

        redLight = red;
        greenLight = green;
        blueLight = blue;

        data = cookieData;

        Initialise();
    }

    public Cookie(ProjectedImageInCookieData precalculatedData, Texture2D precalculatedCookie, Texture2D imageToProject)
    {
        projectedImage = imageToProject;
        if (projectedImage != null)
            imageColours = projectedImage.GetPixels32();
        else
            imageColours = null;

        projectedImageData = precalculatedData;
        if (projectedImageData.colour)
            imageType = ImageType.Colour;

        // create cookies
        redCookie   = new Texture2D(precalculatedData.textureSize, precalculatedData.textureSize, TextureFormat.Alpha8, false);
        greenCookie = new Texture2D(precalculatedData.textureSize, precalculatedData.textureSize, TextureFormat.Alpha8, false);
        blueCookie  = new Texture2D(precalculatedData.textureSize, precalculatedData.textureSize, TextureFormat.Alpha8, false);
        redCookie.wrapMode = greenCookie.wrapMode = blueCookie.wrapMode = TextureWrapMode.Clamp;

        // copy blacks from precalculated cookie
        Graphics.CopyTexture(precalculatedCookie, redCookie);
        if (imageType == ImageType.Colour)
        {
            Graphics.CopyTexture(precalculatedCookie, greenCookie);
            Graphics.CopyTexture(precalculatedCookie, blueCookie);
        }

        redColors = redCookie.GetPixels32();
        greenColors = greenCookie.GetPixels32();
        blueColors = blueCookie.GetPixels32();

        UpdateCookie(false);
    }

    /// <summary>
    /// Creates the Cookie textures and gets the pixel buffers of each
    /// </summary>
    void CreateTexture()
    {
        redCookie   = new Texture2D(textureSize, textureSize, TextureFormat.Alpha8, false);
        greenCookie = new Texture2D(textureSize, textureSize, TextureFormat.Alpha8, false);
        blueCookie  = new Texture2D(textureSize, textureSize, TextureFormat.Alpha8, false);
        redCookie.wrapMode = greenCookie.wrapMode = blueCookie.wrapMode = TextureWrapMode.Clamp;

        redColors   = redCookie.GetPixels32();
        greenColors = greenCookie.GetPixels32();
        blueColors  = blueCookie.GetPixels32();
    }

    /// <summary>
    /// Calculates light cone angle, draws the first Cookie, assigns cookie to light.
    /// TODO: assign cookies to lights in ProjectorSim class - will allow light cookies to be switched between CookieCreators, for slideshow of several images
    /// </summary>
    void Initialise()
    {
        // calculate angle of light cone from throw ratio and possible lens shift amount
        imageWidth = distance / data.ratio;
        imageHeight = imageWidth / data.aspect;

        // calculate shift **IN METERS**
        float shift_H = imageWidth * (data.shift_h / 200.0f);
        float shift_V = imageHeight * (data.shift_v / 200.0f);

        // calculate how far the image can move with full lens shift applied (in meters from lens centre)
        float imageLimit_h = (imageWidth / 2.0f) + Mathf.Abs(shift_H);
        float imageLimit_v = (imageHeight / 2.0f) + Mathf.Abs(shift_V);
        maxImageEdgeDistance = Mathf.Max(imageLimit_h, imageLimit_v);

        metersToPixels = textureSize / (maxImageEdgeDistance * 2f);

        // Calculate the light angle required
        float spotAngle = Mathf.Atan(maxImageEdgeDistance / distance) * 2 * Mathf.Rad2Deg;
        redLight.spotAngle = greenLight.spotAngle = blueLight.spotAngle = spotAngle;

        // draw the cookie(s)
        UpdateCookie();
    }

    /// <summary>
    /// Called when image size/pos is changed and the image shape in the cookie will change
    /// </summary>
    /// <param name="cookieData"></param>
    public void Reinitialise(CookieData cookieData)
    {
        data = cookieData;
        Initialise();
    }

    /// <summary>
    /// Calculates where the image will be in the cookie and creates the whole cookie texture
    /// </summary>
    void UpdateCookie(bool doBlacks = true)
    {
        if (doBlacks)
        {
            float resultWidth, resultHeight;
            resultWidth = distance / data.ratio;
            resultHeight = resultWidth / data.aspect;

            // calculate image position in pixels

            // calculate shift in meters
            Vector2 shift = new Vector2(resultWidth * (data.shift_h / 200.0f),
                                        resultHeight * (data.shift_v / 200.0f));

            // position of image in meters, relative to projector centre
            float imageLeftMeters = maxImageEdgeDistance - (resultWidth / 2.0f) + shift.x;
            float imageTopMeters = maxImageEdgeDistance - (resultHeight / 2.0f) + shift.y;

            // posisiton of image in the cookie texture
            int imageLeftPixels = (int)(imageLeftMeters * metersToPixels);
            int imageTopPixels = (int)(imageTopMeters * metersToPixels);

            // size of image in the cookie texture
            int imageWidthPixels = (int)(resultWidth * metersToPixels);
            int imageHeightPixels = (int)(resultHeight * metersToPixels);

            // keystone width
            float keystoneH = data.keystone_h / 100f;
            if (keystoneH < 0)
                keystoneH *= -1;
            float keystoneMinWidth = imageWidthPixels * (1f - keystoneH);

            float keystoneV = data.keystone_v / 100f;
            if (keystoneV < 0)
                keystoneV *= -1;
            float keystoneMinHeight = imageHeightPixels * (1f - keystoneV);

            // data about where the image is located in the cookie
            projectedImageData = new ProjectedImageInCookieData(imageLeftPixels, imageTopPixels, imageWidthPixels, imageHeightPixels,
                                                                keystoneMinWidth, keystoneMinHeight,
                                                                data.keystone_h < 0, data.keystone_v < 0,
                                                                imageType == ImageType.Colour, textureSize);
        }
        int x, y, pi_x, pi_y;
        int rowWidth, colHeight;
        float f;
        float rowProgress, colProgress;

        byte red, green, blue;
        red = green = blue = 255;

        // set pixel colors
        for (int i = 0; i < redColors.Length; i++)
        {
            x = i % projectedImageData.textureSize;
            y = i / projectedImageData.textureSize;

            // if no H keystone, make row width constant
            if (projectedImageData.imageWidthInCookie == projectedImageData.keystoneMinWidth)
            {
                rowWidth = projectedImageData.imageWidthInCookie;
            }
            else
            {
                // calculate row width after keystone correction
                float verticalProgress = (y - projectedImageData.imageTopEdgeInCookie) / (float)projectedImageData.imageHeightInCookie;
                if (projectedImageData.keystoneH_flip)
                    verticalProgress = 1f - verticalProgress;
                rowWidth = Mathf.RoundToInt(Mathf.Lerp(projectedImageData.imageWidthInCookie, projectedImageData.keystoneMinWidth, verticalProgress));
            }
            rowProgress = (x - (projectedImageData.imageCentreH - rowWidth / 2)) / (float)rowWidth;

            // if no V keystone, make col height constant
            if (projectedImageData.imageHeightInCookie == projectedImageData.keystoneMinHeight)
            {
                colHeight = projectedImageData.imageHeightInCookie;
            }
            else
            {
                // calculate column height after keystone correction
                float horizontalProgress = (x - projectedImageData.imageLeftEdgeInCookie) / (float)projectedImageData.imageWidthInCookie;
                if (projectedImageData.keystoneV_flip)
                    horizontalProgress = 1f - horizontalProgress;
                colHeight = Mathf.RoundToInt(Mathf.Lerp(projectedImageData.imageHeightInCookie, projectedImageData.keystoneMinHeight, horizontalProgress));
            }
            colProgress = (y - (projectedImageData.imageCentreV - colHeight / 2)) / (float)colHeight;
            
                // inside the image row?
            if (y > projectedImageData.imageCentreV - colHeight / 2 && y < projectedImageData.imageCentreV + colHeight / 2 &&
                // inside the image column?
                x > projectedImageData.imageCentreH -rowWidth/2 && x < projectedImageData.imageCentreH-1 +rowWidth/2)
            {
                // white if no projected image
                if (projectedImage == null)
                {
                    redColors[i]   = new Color32(255, 255, 255, 255);

                    // Also set green and blue channels otherwise we get artefacts if colour box is checked
                    if (imageType == ImageType.Colour)
                    {
                        greenColors[i] = new Color32(255, 255, 255, 255);
                        blueColors[i] = new Color32(255, 255, 255, 255);
                    }
                }
                else // use the given texture
                {
                    // select which pixel to take from the image
                    pi_x = Mathf.RoundToInt(Mathf.Lerp(0, projectedImage.width - 1, rowProgress));
                    pi_y = Mathf.RoundToInt(Mathf.Lerp(0, projectedImage.height - 1, colProgress));
                    int flatindex = (projectedImage.width * pi_y) + pi_x;
                    
                    // Colour or greyscale?
                    switch (imageType)
                    {
                        case ImageType.Colour:
                            red = (byte)Mathf.Clamp((imageColours[flatindex].r), 0, 255);
                            redColors[i] = new Color32(255, 255, 255, red);
                            green = (byte)Mathf.Clamp((imageColours[flatindex].g), 0, 255);
                            greenColors[i] = new Color32(255, 255, 255, green);
                            blue = (byte)Mathf.Clamp((imageColours[flatindex].b), 0, 255);
                            blueColors[i] = new Color32(255, 255, 255, blue);
                            break;
                        default: // greyscale
                            f = ((float)imageColours[flatindex].r +
                                        imageColours[flatindex].g +
                                        imageColours[flatindex].b) / 3f;
                            red = (byte)Mathf.Clamp((int)f, 0, 255);
                            redColors[i] = new Color32(255, 255, 255, red);
                            break;
                    }
                }
            }
            else if (doBlacks) // BLACK PIXELS
            {
                redColors[i] = new Color32(255, 255, 255, 0);
                if (imageType == ImageType.Colour) 
                {
                    greenColors[i] = new Color32(255, 255, 255, 0);
                    blueColors[i]  = new Color32(255, 255, 255, 0);
                }
            }
        }

        // apply new colours
        redCookie.SetPixels32(redColors);
        redCookie.Apply();
        if (imageType == ImageType.Colour)
        {
            greenCookie.SetPixels32(greenColors);
            greenCookie.Apply();
            blueCookie.SetPixels32(blueColors);
            blueCookie.Apply();
        }
    }


    /// <summary>
    /// Set the image to project (does NOT cause a redraw - call Reinitialise to redraw)
    /// </summary>
    /// <param name="image"></param>
    /// <param name="pixels"></param>
    public void SetProjectedImage(Texture2D image)
    {
        projectedImage = image;
        if (image != null)
            imageColours = image.GetPixels32();
    }
    public void RemoveProjectedImage() { projectedImage = null; imageColours = null; }
    public void SetProjectedImageType(ImageType type)
    {
        imageType = type;
        if (imageType == ImageType.Colour) // colour mode?
            redLight.color = Color.red;
        else
            redLight.color = Color.white;
        greenLight.enabled = blueLight.enabled = imageType == ImageType.Colour;
    }
    public void SetCookieSize(int newSize)
    {
        textureSize = newSize;
        CreateTexture();
        Initialise();
    }

    public Texture2D GetRedCookie()   { return redCookie;   }
    public Texture2D GetGreenCookie() { return greenCookie; }
    public Texture2D GetBlueCookie()  { return blueCookie;  }

    public ProjectedImageInCookieData GetProjectedImageData() { return projectedImageData; }
}
