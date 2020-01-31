using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JdSuite.Common
{
    public static class FileUtils
    {
        /// 
        /// Seeks the position of a string of data in the file.
        /// 
        /// The path to a file.
        /// A string to search for.
        /// The position of the string or -1 if 
        /// the string could not be found.
        public static long Seek(string file, string searchString)
        {
            //open filestream to perform a seek
            using (System.IO.FileStream fs = System.IO.File.OpenRead(file))
            {
                return Seek(fs, searchString,int.MaxValue);
            }
        }

 


        /// 
        /// Seeks the position of a string in the file stream. 
        /// It will advance the position of the stream.
        /// 
        /// An open file stream.
        /// A string to search for.
        /// The position of the string or -1 if 
        /// the string could not be found.
        public static long Seek(System.IO.FileStream fileStream, string searchString, int MaxCount)
        {
            char[] search = searchString.ToCharArray();
            long result = -1, position = 0, stored = -1,
            begin = fileStream.Position;
            int c;

            //read byte by byte
            while ((c = fileStream.ReadByte()) != -1)
            {
                if(fileStream.Position-begin>MaxCount)
                {
                    result = -1;
                    break;
                }
                //check if data in array matches
                if ((char)c == search[position])
                {
                    //if charater matches first character of 
                    //seek string, store it for later
                    if (stored == -1 && position > 0 && (char)c == search[0])
                    {
                        stored = fileStream.Position;
                    }

                    //check if we're done
                    if (position + 1 == search.Length)
                    {
                        //correct position for array lenth
                        result = fileStream.Position - search.Length;
                        //set position in stream
                        fileStream.Position = result;
                        break;
                    }

                    //advance position in the array
                    position++;
                }
                //no match, check if we have a stored position
                else if (stored > -1)
                {
                    //go to stored position + 1
                    fileStream.Position = stored + 1;
                    position = 1;
                    stored = -1; //reset stored position!
                }
                //no match, no stored position, reset array
                //position and continue reading
                else
                {
                    position = 0;
                }
            }

            //reset stream position if no match has been found
            if (result == -1)
            {
                fileStream.Position = begin;
            }

            return result;
        }
    }
}
