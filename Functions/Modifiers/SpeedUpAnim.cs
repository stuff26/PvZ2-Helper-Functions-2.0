using UniversalMethods;
using XflComponents;

namespace HelperFunctions.Functions.Packages
{
    public static class SpeedUpAnim
    {
        public static void Function()
        {
            // Get the symbol to edit + its path
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter the symbol you want to edit", separateLines:true);
            var (symbolPath, symbol)  = UserPrompts.AskForSymbolItem();

            // Get how much to speed up the symbol by
            UM.PrintColoredText([
                (ConsoleColor.DarkCyan, "Enter how much you want to speed up the symbol"),
                (ConsoleColor.Yellow, " (integers > 1 only)\n")
            ]);
            int speedUpAmount = UserPrompts.AskForInt(min:2);

            // Speed up each layer
            var layers = symbol.Timeline.Layers;
            var editLayers = new ProgressChecker("Editing layers...", layers.Count);
            foreach (var layer in layers)
            {
                SpeedUpLayer(layer, speedUpAmount);
                editLayers.AddOne();
            }
            symbol.Timeline.RemoveEmptyLayers(); // Remove leftover empty layers

            // Save document
            UM.PrintColoredText(ConsoleColor.Green, "Saving symbol... ");
            XmlMethods.SaveXmlDocument(symbolPath, symbol, SymbolItem.serializer);
            ProgressChecker.WriteFinished();
        }

        private static void SpeedUpLayer(AnimateLayer layer, double speedUpAmount)
        {
            int frameTracker = 1;
            foreach (var frame in layer.Frames)
            {
                // Loop through this frame same time as duration of it
                var timestoLoop = frame.Duration;
                for (int i = 0; i < timestoLoop; i++)
                {
                    // If the current num is 1, the frame will be ensured to not be removed
                    if (frameTracker == 1)
                    {
                        frameTracker++;
                        continue;
                    }

                    // Reduce duration of the frame if this original index is meant to be skipped, 
                    frame.Duration--;
                    if (frameTracker != speedUpAmount)
                    {
                        frameTracker++;
                    }

                    // Reset if `speedUpAmount` number of frames have been counted
                    else
                    {
                        frameTracker = 1;
                    }
                }
            }

            // Remove frames with a duration of < 0 from layer
            layer.RemoveZeroDurationFrames();

            // Fix the indexes of the frames
            foreach (var frame in layer.Frames)
            {
                double index = frame.Index;
                frame.Index = (int) Math.Round((index / speedUpAmount) + 0.5); // Round up index
            }
        }
    }
}