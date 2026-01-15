
using UniversalMethods;
using XflComponents;

namespace HelperFunctions.Functions.Packages
{
    public class SpeedUpAnim
    {
        public static void Function()
        {
            // Get the symbol to edit + its path
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the symbol you want to edit");
            var (symbolPath, symbol)  = UM.AskForSymbolItem();

            // Get how much to speed up the symbol by
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter how much you want to speed up the symbol (integers > 1 only)");
            int speedUpAmount = UM.AskForInt(2);

            // Speed up each layer
            var layers = symbol.Timeline!.Layers;
            Console.ForegroundColor = ConsoleColor.Green;
            var editLayers = new ProgressChecker("Editing layers... ", layers.Count);
            foreach (var layer in layers)
            {
                SpeedUpLayer(layer, speedUpAmount);
                editLayers.AddOne();
            }
            symbol.Timeline.RemoveEmptyLayers(); // Remove leftover empty layers

            // Save document
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Saving symbol... ");
            UM.SaveXmlDocument(symbolPath, symbol, UM.DummyXDocument, SymbolItem.serializer);
            ProgressChecker.WriteFinished();
        }

        public static void SpeedUpLayer(AnimateLayer layer, float speedUpAmount)
        {
            // Setup
            int layerLength = layer.GetLayerLength();
            var frameIndexes = new AnimateFrame[layerLength]; // Keeps track of the frame at each index point

            // Fill in frameIndexes with proper frames at the right indexes
            foreach (var frame in layer.Frames)
            {
                int index = frame.index;
                frameIndexes[index] = frame;
                for (int tempNum = frame.duration; tempNum > 1; tempNum--)
                {
                    frameIndexes[index+tempNum-1] = frame;
                }
            }

            // Remove unnecessary frames
            int currentNum = 1;
            foreach (var frame in frameIndexes)
            {
                // If the current num is 1, the frame will be ensured to not be removed
                if (currentNum == 1)
                {
                    currentNum++;
                    continue;
                }

                // Reduce duration of the frame
                frame.duration--;

                // Reset
                if (currentNum == speedUpAmount)
                {
                    currentNum = 1;
                }
                else
                {
                    currentNum++;
                }
            }

            // Remove frames with a duration of 0 from layer
            layer.RemoveZeroDurationFrames();

            // Fix the indexes of the frames
            foreach (var frame in layer.Frames)
            {
                float index = frame.index;
                frame.index = (int) ((index / speedUpAmount) + 0.5); // Round up index
            }
        }
    }
}