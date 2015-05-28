using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Main : Form
    {
        private byte[] allBytes; // Just read the file into the byte array

        // Interpret the header information
        private int numImages;
        private int numCodeword;
        private byte[] huffmanTable, leafNode;
        private int[] huffmanValue;
        private int lenImage1,lenImage2,lenImage3;
        private int lenBinaryBeforePadding;
        private string compFileInString;
        private Dictionary<int, int> symbol_length_Table;
        private Dictionary<int, int> symbol_value_Table;
        public int[,] HuffDec;
        private decimal[] allDecimal, image1, image2, image3;




        // From the previous question.....


        // File header from byte number 0 to 11
        private string ByteOrder;
        private int FileType;
        private int OffsetIFD1;
        private int NumOfDirectories;

        // Tag information from byte number 0 to 11
        private int SubfileType;
        private int ImageWidth;
        private int ImageLength;
        private int BitsPerSample;
        private int Compression;
        private int PhotometricInterpretation;
        private int StripOffsets;
        private int SamplesPerPixel;
        private int XResolution;
        private int YResolution;
        private int PlanarConfiguration;

        // Necessarily, the following variables are also set global
        private string fileDirectory;    // File location
        private bool file1 = false, file2 = false, file3 = false;
        private int[] buffImage1, buffImage2, buffImage3;
        private byte[] allImageBytes, image1byte, image2byte, image3byte;
        private int pixLength;

        private int[] arrGray;                  // Sequential values of the grayscale image
        private int countArrGray;               // Counts the number of elements in the array
        private int fileLength;                 // The length of a file in byte
        private int red = 0;                    // Indicates the red scale from 0 to 255
        private int green = 0;                  // Sets Green Colour Index
        private int blue = 0;                   // Sets Blue Colour Index
        private int countRow = 1;               // Y value of the single pixel
        private int countColumn = 1;            // X value of the single pixel
        private int[] sorted_histogram;

        public int last_i;
        public int test_value;
        public int maxCodewordLength;






        public Main()
        {
            InitializeComponent();
            // Adjust form1 window size
            this.Width = 1000;
            this.Height = 700;
        }

        public static byte[] ConvertIntToByteArray(int I)
        {
            return BitConverter.GetBytes(I);
        }

        public static double ConvertByteArrayToInt32(byte[] b)
        {
            return BitConverter.ToInt32(b, 0);
        }

        public static int ConvertBinaryStringToInt32(string BS)
        {
            return Convert.ToInt32(BS, 2);
        }

        public static string ConvertByteToBinaryString(byte b)
        {
            if (Convert.ToString(b, 2).Length < 8) {
                return Convert.ToString(b, 2).PadLeft(8, '0');
            }
            return Convert.ToString(b, 2);
        }

        private void interpretHeader()
        {
            // One header information: number of images
            numImages = (int)allBytes[0];
            
            // Another header: number of codewords
            byte[] byte4 = new byte[4];
            Array.Copy(allBytes, 1, byte4, 0, 4);
            numCodeword = (int)ConvertByteArrayToInt32(byte4);

            // Another header: Huffman tree
            huffmanTable = new byte[numCodeword * 9];
            Array.Copy(allBytes, 5, huffmanTable, 0, numCodeword * 9);

            // Create a lookup table for huffman tree
            // int count = 0;
            symbol_length_Table = new Dictionary<int, int>();
            symbol_value_Table = new Dictionary<int, int>();
            for (int i = 0; i < numCodeword * 9; i = i + 9)
            {
                int symbol = (int)huffmanTable[i];
                Array.Copy(huffmanTable, i+1, byte4, 0, 4);
                int huffmanValue = (int)ConvertByteArrayToInt32(byte4);

                Array.Copy(huffmanTable, i+5, byte4, 0, 4);
                int huffmanLength = (int)ConvertByteArrayToInt32(byte4);

                symbol_length_Table.Add(symbol, huffmanLength);
                symbol_value_Table.Add(symbol, huffmanValue);
            }

            // Another header: length of image1
            int startIndex = 5 + numCodeword * 9;
            Array.Copy(allBytes, startIndex, byte4, 0, 4);
            lenImage1 = (int)ConvertByteArrayToInt32(byte4);
            // Another header: length of image2
            startIndex = startIndex + 4;
            Array.Copy(allBytes, startIndex, byte4, 0, 4);
            lenImage2 = (int)ConvertByteArrayToInt32(byte4);
            // Another header: length of image3
            startIndex = startIndex + 4;
            Array.Copy(allBytes, startIndex, byte4, 0, 4);
            lenImage3 = (int)ConvertByteArrayToInt32(byte4);

            // Another header: Original binary string length before padding '0'
            startIndex = startIndex + 4;
            Array.Copy(allBytes, startIndex, byte4, 0, 4);
            lenBinaryBeforePadding = (int)ConvertByteArrayToInt32(byte4);
            
            // Gather binary string of all the images
            startIndex = startIndex + 4;
            // Use StringBuilder for concatenation in tight loops.
            StringBuilder compFile = new System.Text.StringBuilder();
            compFileInString = "";
            string testBinary = ConvertByteToBinaryString(allBytes[startIndex]);
            for (int i = startIndex; i < allBytes.Length; i++) 
            {
                string binary = ConvertByteToBinaryString(allBytes[i]);
                compFile.Append(binary);
            }
            compFileInString = compFile.ToString();
            compFileInString = compFileInString.Substring(0, lenBinaryBeforePadding);
            //label1.Text = lenImage1 + " " + lenImage2 + " " + lenImage3 + " " + lenBinaryBeforePadding + " " + compFileInString.Length + " " + testBinary;

        }

        private void createHuffDecoding() {

            HuffDec = new int[1024,2];
            int j = 0;
            int end = (int)Math.Pow(2, 10);
            for (int i = 0; i < end; i++) 
            {
                if (j==255)
                {
                    HuffDec[i, 0] = symbol_length_Table.Keys.ElementAt(j);
                    HuffDec[i, 1] = symbol_length_Table.Values.ElementAt(j);
                }
                else
                {
                    if (i < symbol_value_Table.Values.ElementAt(j + 1))
                    {
                        HuffDec[i, 0] = symbol_length_Table.Keys.ElementAt(j);
                        HuffDec[i, 1] = symbol_length_Table.Values.ElementAt(j);
                    }
                    else
                    {
                        j++;
                        i--;
                    }
                }
            }
            //label1.Text = "" + HuffDec[7,0] + " " + HuffDec[7,1];
        }

        private int lookup_method()
        {
            allDecimal = new decimal[lenImage1 + lenImage2 + lenImage3];
            // Read characters from the string into the array.
            string x = compFileInString.Substring(0, 10);
            int startIndex1 = 10; // maximum codeword length
            // label1.Text = x;
            int k = 0; // # of symbols decoded
            int intX;
            int length = 0;
            while (true) {
                if (compFileInString.Length == startIndex1+2)
                {
                    intX = ConvertBinaryStringToInt32(x);
                    allDecimal[k] = HuffDec[intX, 0];
                    //label1.Text = "" + allDecimal[0];
                    return 1;
                }
                intX = ConvertBinaryStringToInt32(x);
                allDecimal[k] = HuffDec[intX, 0];
                k++;
                length = HuffDec[intX, 1];
                x = x.Substring(length);
                //label1.Text = "" + x;
                int y = startIndex1;
                int l = length;
                int g = compFileInString.Length;
                string newbits = compFileInString.Substring(startIndex1, length);
                startIndex1 = startIndex1 + length;

                x = x + newbits;
            }
            
            /*
            string g = "12345";
            g = g.Substring(3);
            label1.Text = g;
            return 1;
        */
        }

        private void buildHDR_RGB(int xPixOffset, int yPixOffset)
        {
            int rgbLength = pixLength * 3;
            for (int i = 0; i < rgbLength; i = i + 3)
            {
                red = (buffImage1[i] + buffImage2[i] + buffImage3[i]) / 3;
                green = (buffImage1[i + 1] + buffImage2[i + 1] + buffImage3[i + 1]) / 3;
                blue = (buffImage1[i + 2] + buffImage2[i + 2] + buffImage3[i + 2]) / 3;
                drawRGB(red, green, blue, xPixOffset, yPixOffset);
            }
        }

        private void drawBuff(int nImage, byte[] buff, int xPixOffset, int yPixOffset)
        {
            /*
             *  From here, extract header information
             */
            if (nImage == 4){
                buildHDR_RGB(xPixOffset, yPixOffset);
            }
            else
            {
                ASCIIEncoding ascii = new ASCIIEncoding();

                // Identify whether the order of bytes are little-endian or big-endian
                // If "II", then it is little-endian
                byte[] bByteOrder = { buff[0], buff[1] };
                ByteOrder = ascii.GetString(bByteOrder);

                // Identify whether the order of bytes are little-endian or big-endian
                // If 42, then it is TIFF file format
                byte[] bFileType = { buff[2], buff[3] };
                if (ByteOrder == "II")
                    bFileType.Reverse();
                FileType = BitConverter.ToInt16(bFileType, 0);

                // Identify the first IFD offset and store the value into OffsetIFD1
                byte[] bOffsetIFD1 = { buff[4], buff[5], buff[6], buff[7] };
                if (ByteOrder == "II")
                    bOffsetIFD1.Reverse();
                OffsetIFD1 = BitConverter.ToInt32(bOffsetIFD1, 0);

                // Identify the first IFD offset and store the value into OffsetIFD1
                byte[] bNumOfDirectories = { buff[8], buff[9] };
                if (ByteOrder == "II")
                    bNumOfDirectories.Reverse();
                NumOfDirectories = BitConverter.ToInt16(bNumOfDirectories, 0);

                identify_tag(NumOfDirectories, buff); // Call the "identify_tag" methods to dig IFD information
                pixLength = ImageWidth * ImageLength;
                int pixEnd = StripOffsets + pixLength * 3;

                switch (nImage)
                {
                    case 1:
                        buffImage1 = new int[pixLength * 3];
                        file1 = true;
                        break;
                    case 2:
                        buffImage2 = new int[pixLength * 3];
                        file2 = true;
                        break;
                    case 3:
                        buffImage3 = new int[pixLength * 3];
                        file3 = true;
                        break;
                }

                // Reads file from buff
                int j = 0;
                for (int i = StripOffsets; i < pixEnd; i = i + 3)
                {
                    red = buff[i];
                    green = buff[i + 1];
                    blue = buff[i + 2];

                    switch (nImage)
                    {
                        case 1:
                            buffImage1[j] = red;
                            buffImage1[j + 1] = green;
                            buffImage1[j + 2] = blue;
                            break;
                        case 2:
                            buffImage2[j] = red;
                            buffImage2[j + 1] = green;
                            buffImage2[j + 2] = blue;
                            break;
                        case 3:
                            buffImage3[j] = red;
                            buffImage3[j + 1] = green;
                            buffImage3[j + 2] = blue;
                            break;
                    }
                    j = j + 3;
                    drawRGB(red, green, blue, xPixOffset, yPixOffset);
                }
            }
        }

        private void drawRGB(int red, int green, int blue, int xPixOffset, int yPixOffset)
        {
            // Declares colour variable
            Color myRgbColor = new Color();

            // Create color and brush
            myRgbColor = Color.FromArgb(red, green, blue);
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(myRgbColor);
            System.Drawing.Graphics formGraphics;
            formGraphics = this.CreateGraphics();

            // Choose the location where you would like to draw a rectangle
            // And pick the size of a rectangle (one pixel)
            formGraphics.FillRectangle(myBrush, new Rectangle((countColumn + xPixOffset), (countRow + yPixOffset), 1, 1));
            countColumn++;
            myBrush.Dispose();
            formGraphics.Dispose();

            // If one row is filled with ImageWidth number of rectangles with one pixel, go to next row
            if (countColumn % ImageWidth == 1)
            {
                countRow++;
                countColumn = 1;
            }
        }

        private void identify_tag(int number, byte[] buff)
        {
            int tagStarts = OffsetIFD1 + 2;
            int tagEnds = OffsetIFD1 + 2 + number * 12; // 142 in our sample file
            while (tagStarts < tagEnds)
            {
                // Extracts Four Field Informations From 12 Byte IFD
                int tagID;
                int Type;
                int Count;
                int Value;

                int i = tagStarts;
                byte[] bTagID = { buff[i], buff[i + 1] };
                byte[] bType = { buff[i + 2], buff[i + 3] };
                byte[] bCount = { buff[i + 4], buff[i + 5], buff[i + 6], buff[i + 7] };
                byte[] bValue = { buff[i + 8], buff[i + 9], buff[i + 10], buff[i + 11] };

                if (ByteOrder == "II")
                {
                    bTagID.Reverse();
                    bType.Reverse();
                    bCount.Reverse();
                    bValue.Reverse();
                }

                tagID = BitConverter.ToInt16(bTagID, 0);
                Type = BitConverter.ToInt16(bType, 0);
                Count = BitConverter.ToInt32(bCount, 0);
                Value = BitConverter.ToInt32(bValue, 0);

                if (tagID == 256)
                    ImageWidth = Value;
                if (tagID == 257)
                    ImageLength = Value;
                if (tagID == 273)
                    StripOffsets = Value;

                tagStarts = tagStarts + 12;
            }
        }

        private void btnOpenTHR_Click(object sender, EventArgs e)
        {
            //// Create an instance of the open file dialog box.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "THR Compressed Files (.thr)|*.thr";
            openFileDialog1.FilterIndex = 1;

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                // Saves file location and reads it into a byte array
                string fileDirectory = openFileDialog1.FileName;
                allBytes = File.ReadAllBytes(fileDirectory);
                int fileLength = allBytes.Length;

                // Gather all the header information
                interpretHeader();
                createHuffDecoding();
                int validity = lookup_method();

                //label1.Text = "" + allDecimal[0] + " " + allDecimal[363972] + " " + allDecimal[725828];
                image1 = allDecimal.Skip(0).Take(lenImage1).ToArray();
                image2 = allDecimal.Skip(lenImage1).Take(lenImage2).ToArray();
                image3 = allDecimal.Skip(lenImage1 + lenImage2).Take(lenImage3).ToArray();
                // label1.Text = "" + image1[0] + " " + image2[0] + " " + image3[0];

                byte[] image1byte = new byte[image1.Length];
                byte[] image2byte = new byte[image2.Length];
                byte[] image3byte = new byte[image3.Length];

                for (int i = 0; i < image1.Length; i++) 
                {
                    image1byte[i] = (byte)image1[i];
                }
                for (int i = 0; i < image2.Length; i++)
                {
                    image2byte[i] = (byte)image2[i];
                }
                for (int i = 0; i < image3.Length; i++)
                {
                    image3byte[i] = (byte)image3[i];
                }

                drawBuff(1, image1byte, 150, 20);
                drawBuff(2, image2byte, 550, -270);
                drawBuff(3, image3byte, 150, -250);
                drawBuff(4, null, 550, -550);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Detail form2 = new Detail(HuffDec);
            form2.Show();
        }


    }
}
