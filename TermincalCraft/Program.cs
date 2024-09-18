// See https://aka.ms/new-console-template for more information

using TermincalCraft;


var withColor = args.Contains("--color");

Console.Clear();

var game = new CraftGame(new()
{
    UseColors = withColor
});
game.WaitForExit();