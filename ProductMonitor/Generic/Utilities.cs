namespace ProductMonitor.Generic
{
    static class Utilities<type>
    {
        /// <summary>
        /// Returns the array without null values
        /// </summary>
        static public type[] RemoveNulls(type[] array)
        {
         /*   int numNulls = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    numNulls++;
                }
            }
            
            if (numNulls == 0) {
                return array;
            } else {

                type[] arrayWithoutNulls = new type[array.Length - numNulls];
                int currentIndex = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] != null)
                    {
                        arrayWithoutNulls[currentIndex] = array[i];
                        currentIndex++;
                    }
                }
                return arrayWithoutNulls; 
           
            }  */

            return array;
            //TODO: remove the call to this function from the program
        }

    }
}
