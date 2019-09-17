using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

// Created in 2012 by Jakob Krarup (www.xnafan.net).
// Use, alter and redistribute this code freely,
// but please leave this comment :)


namespace XnaFan.ImageComparison
{

    /// <summary>
    /// A class with extensionmethods for comparing images
    /// </summary>
    public static class ImageTool
    {
        private static PathGrayscaleTupleComparer Comparer = new PathGrayscaleTupleComparer();


        public static List<bool> GetHash(string image1Path)
        {
            if (!File.Exists(image1Path))
            {
                throw new ArgumentException("Нет файла или нет доступа к файлу " + image1Path, "image1Path");
            }
            Image bmpSource = Image.FromFile(image1Path);
            List<bool> lResult = new List<bool>();
            //create new image with 16x16 pixel
            Bitmap bmpMin = new Bitmap(bmpSource, new Size(16, 16));
            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    //reduce colors to true / false                
                    lResult.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f);
                }
            }
            return lResult;
                
        }

        public static string HashGenerator(string image1path)
        {
            List<bool> BoolArr = GetHash(image1path);
            List<byte> byteArray = new List<byte>();
            var boolList = BoolArr;

            var nChunks = 10;
            var totalLength = boolList.Count();
            var chunkLength = (int)Math.Ceiling(totalLength / (double)nChunks);
            var parts = Enumerable.Range(0, 10)
                                  .Select(i => boolList.Skip(i * chunkLength)
                                                     .Take(chunkLength)
                                                     .ToList())
                                  .ToList();
            foreach (var part in parts)
            {
                byteArray.Add(ConvertBoolArrayToByte(part.ToArray()));
            }
            
            return BitConverter.ToString(byteArray.ToArray());
        }

        public static string HashDiff(string image1Path, string image2Path)
        {
            List<bool> iHash1 = GetHash(image1Path);
            List<bool> iHash2 = GetHash(image2Path);

            int equalElements = iHash1.Zip(iHash2, (i, j) => i == j).Count(eq => eq);
            double flow = 100 - (((double)equalElements / 256) * 100);

            return flow.ToString();
        }

        /// <summary>
        /// Gets the difference between two images as a percentage
        /// </summary>
        /// <returns>The difference between the two images as a percentage</returns>
        /// <param name="image1Path">The path to the first image</param>
        /// <param name="image2Path">The path to the second image</param>
        /// <param name="threshold">How big a difference (out of 255) will be ignored - the default is 3.</param>
        /// <returns>The difference between the two images as a percentage</returns>
        public static float GetPercentageDifference(string image1Path, string image2Path, byte threshold = 3)
        {
            if (CheckIfFileExists(image1Path) && CheckIfFileExists(image2Path))
            {
                Image img1 = Image.FromFile(image1Path);
                Image img2 = Image.FromFile(image2Path);

                float difference = img1.PercentageDifference(img2, threshold);

                img1.Dispose();
                img2.Dispose();

                return difference;
            }
            else return -1;
        }

        /// <summary>
        /// Gets the difference between two images as a percentage
        /// </summary>
        /// <returns>The difference between the two images as a percentage</returns>
        /// <param name="image1Path">The path to the first image</param>
        /// <param name="image2Path">The path to the second image</param>
        /// <param name="threshold">How big a difference (out of 255) will be ignored - the default is 3.</param>
        /// <returns>The difference between the two images as a percentage</returns>
        public static float GetBhattacharyyaDifference(string image1Path, string image2Path)
        {
            if (CheckIfFileExists(image1Path) && CheckIfFileExists(image2Path))
            {
                Image img1 = Image.FromFile(image1Path);
                Image img2 = Image.FromFile(image2Path);

                float difference = img1.BhattacharyyaDifference(img2);

                img1.Dispose();
                img2.Dispose();

                return difference;
            }
            else return -1;
        }


        /// <summary>
        /// Find all duplicate images in a folder, and possibly subfolders
        /// IMPORTANT: this method assumes that all files in the folder(s) are images!
        /// </summary>
        /// <param name="folderPath">The folder to look for duplicates in</param>
        /// <param name="checkSubfolders">Whether to look in subfolders too</param>
        /// <returns>A list of all the duplicates found, collected in separate Lists (one for each distinct image found)</returns>
        public static List<List<string>> GetDuplicateImages(string folderPath, bool checkSubfolders)
        {
            var imagePaths = Directory.GetFiles(folderPath, "*.*", checkSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            return GetDuplicateImages(imagePaths);
        }

        /// <summary>
        /// Find all duplicate images from in list
        /// </summary>
        /// <param name="pathsOfPossibleDuplicateImages">The paths to the images to check for duplicates</param>
        /// <returns>A list of all the duplicates found, collected in separate Lists (one for each distinct image found)</returns>
        public static List<List<string>> GetDuplicateImages(IEnumerable<string> pathsOfPossibleDuplicateImages)
        {
            var imagePathsAndGrayValues = GetSortedGrayscaleValues(pathsOfPossibleDuplicateImages);
            var duplicateGroups = GetDuplicateGroups(imagePathsAndGrayValues);

            var pathsGroupedByDuplicates = new List<List<string>>();
            foreach (var list in duplicateGroups)
            {
                pathsGroupedByDuplicates.Add(list.Select(tuple => tuple.Item1).ToList());
            }

            return pathsGroupedByDuplicates;
        }

        #region Helpermethods

        private static List<Tuple<string, byte[,]>> GetSortedGrayscaleValues(IEnumerable<string> pathsOfPossibleDuplicateImages)
        {
            var imagePathsAndGrayValues = new List<Tuple<string, byte[,]>>();
            foreach (var imagePath in pathsOfPossibleDuplicateImages)
            {
                using (Image image = Image.FromFile(imagePath))
                {
                    byte[,] grayValues = image.GetGrayScaleValues();
                    var tuple = new Tuple<string, byte[,]>(imagePath, grayValues);
                    imagePathsAndGrayValues.Add(tuple);
                }
            }

            imagePathsAndGrayValues.Sort(Comparer);
            return imagePathsAndGrayValues;
        }

        private static List<List<Tuple<string, byte[,]>>> GetDuplicateGroups(List<Tuple<string, byte[,]>> imagePathsAndGrayValues)
        {
            var duplicateGroups = new List<List<Tuple<string, byte[,]>>>();
            var currentDuplicates = new List<Tuple<string, byte[,]>>();

            foreach (Tuple<string, byte[,]> tuple in imagePathsAndGrayValues)
            {
                if (currentDuplicates.Any() && Comparer.Compare(currentDuplicates.First(), tuple) != 0)
                {
                    if (currentDuplicates.Count > 1)
                    {
                        duplicateGroups.Add(currentDuplicates);
                        currentDuplicates = new List<Tuple<string, byte[,]>>();
                    }
                    else
                    {
                        currentDuplicates.Clear();
                    }
                }

                currentDuplicates.Add(tuple);
            }
            if (currentDuplicates.Count > 1)
            {
                duplicateGroups.Add(currentDuplicates);
            }
            return duplicateGroups;
        }

        private static byte ConvertBoolArrayToByte(bool[] source)
        {
            byte result = 0;
            int index = 8 - source.Length;

            foreach (bool b in source)
            {
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }

        private static bool CheckIfFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File '" + filePath + "' not found!");
            }
            return true;
        }
        #endregion

    }
}