using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

using Microsoft.UI;
using Microsoft.Windows.Widgets.Providers;

using Windows.Storage;
using Windows.UI;

namespace MyNotes.Widget;

public partial class WidgetProvider : IWidgetProvider
{
  public static Dictionary<string, WidgetInfo> RunningWidgets { get; } = new();
  public static ManualResetEvent EmptyWidgetListEvent { get; } = new(false);

  public void CreateWidget(WidgetContext widgetContext)
  {
    var widgetId = widgetContext.Id;
    var widgetName = widgetContext.DefinitionId;
    WidgetInfo runningWidgetInfo = new() { WidgetId = widgetId, WidgetName = widgetName };
    RunningWidgets[widgetId] = runningWidgetInfo;

    // Update the widget
    UpdateWidget(runningWidgetInfo);
  }

  public void DeleteWidget(string widgetId, string customState)
  {
    RunningWidgets.Remove(widgetId);

    if (RunningWidgets.Count == 0)
    {
      EmptyWidgetListEvent.Set();
    }
  }

  public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
  {
    var verb = actionInvokedArgs.Verb;
    var widgetId = actionInvokedArgs.WidgetContext.Id;
    if (RunningWidgets.TryGetValue(widgetId, out var localWidgetInfo))
    {
      if (verb == "previous")
      {
        localWidgetInfo.CustomState--;
        UpdateWidget(localWidgetInfo);
      }
      else if (verb == "next")
      {
        localWidgetInfo.CustomState++;
        UpdateWidget(localWidgetInfo);
      }
    }
  }

  public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
  {
    var widgetContext = contextChangedArgs.WidgetContext;
    var widgetId = widgetContext.Id;
    var widgetSize = widgetContext.Size;
    if (RunningWidgets.TryGetValue(widgetId, out WidgetInfo? localWidgetInfo))
    {
      UpdateWidget(localWidgetInfo);
    }
  }

  public void Activate(WidgetContext widgetContext)
  {
    var widgetId = widgetContext.Id;

    if (RunningWidgets.TryGetValue(widgetId, out WidgetInfo? localWidgetInfo))
    {
      localWidgetInfo.IsActive = true;
      UpdateWidget(localWidgetInfo);
    }
  }

  public void Deactivate(string widgetId)
  {
    if (RunningWidgets.TryGetValue(widgetId, out WidgetInfo? localWidgetInfo))
    {
      localWidgetInfo.IsActive = false;
    }
  }

  List<Color> BackgroundColors =
  [
    Colors.LightGoldenrodYellow,
    Colors.Black,
    Colors.LightPink,
    new Color() { A = 64, R = 150, G = 150, B = 240 },
    new Color() { A = 64, R = 17, G = 135, B = 34 },
    new Color() { A = 16, R = 17, G = 135, B = 34 }
  ];
  public void UpdateWidget(WidgetInfo localWidgetInfo)
  {
    WidgetUpdateRequestOptions updateOptions = new(localWidgetInfo.WidgetId);

    string? templateJson = null;
    string? dataJson = null;
    if (localWidgetInfo.WidgetName == "Note_Widget")
    {
      NoteWidgets noteWidgets = new()
      {
        Notes =
        [
          "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim.\r\n\r\nAliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna.\r\n\r\nSed consequat, leo eget bibendum sodales, augue velit cursus nunc, quis gravida magna mi a libero. Fusce vulputate eleifend sapien. Vestibulum purus quam, scelerisque ut, mollis sed, nonummy id, metus. Nullam accumsan lorem in dui. Cras ultricies mi eu turpis hendrerit fringilla. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; In ac dui quis mi consectetuer lacinia. Nam pretium turpis et arcu. Duis arcu tortor, suscipit eget, imperdiet nec, imperdiet iaculis, ipsum. Sed aliquam ultrices mauris. Integer ante arcu, accumsan a, consectetuer eget, posuere ut, mauris. Praesent adipiscing. Phasellus ullamcorper ipsum rutrum nunc. Nunc nonummy metus.",
          "Some years ago—never mind how long precisely—having little or no money in my purse, and nothing particular to interest me on shore, I thought I would sail about a little and see the watery part of the world.\r\nIt is a way I have of driving off the spleen and regulating the circulation.\r\nWhenever I find myself growing grim about the mouth; whenever it is a damp, drizzly November in my soul; whenever I find myself involuntarily pausing before coffin warehouses, and bringing up the rear of every funeral I meet; and especially whenever my hypos get such an upper hand of me, that it requires a strong moral principle to prevent me from deliberately stepping into the street, and methodically knocking people's hats off—then, I account it high time to get to sea as soon as I can.\r\nThis is my substitute for pistol and ball.\r\nWith a philosophical flourish Cato throws himself upon his sword; I quietly take to the ship.\r\nThere is nothing surprising in this.\r\nIf they but knew it, almost all men in their degree, some time or other, cherish very nearly the same feelings towards the ocean with me.\r\nThere now is your insular city of the Manhattoes, belted round by wharves as Indian isles by coral reefs—commerce surrounds it with her surf.\r\nRight and left, the streets take you waterward.\r\nIts extreme downtown is the battery, where that noble mole is washed by waves, and cooled by breezes, which a few hours previous were out of sight of land.\r\nLook at the crowds of water-gazers there.\r\nCircumambulate the city of a dreamy Sabbath afternoon.\r\nGo from Corlears Hook to Coenties Slip, and from thence, by Whitehall, northward.\r\nWhat do you see?—Posted like silent sentinels all around the town, stand thousands upon thousands of mortal men fixed in ocean reveries.\r\nSome leaning against the spiles; some seated upon the pier-heads; some looking over the bulwarks of ships from China; some high aloft in the rigging, as if striving to get a still better seaward peep.\r\nBut these are all landsmen; of week days pent up in lath and plaster—tied to counters, nailed to benches, clinched to desks.\r\nHow then is this?\r\nAre the green fields gone?\r\nWhat do they here?\r\nBut look!\r\nhere come more crowds, pacing straight for the water, and seemingly bound for a dive.\r\nStrange!\r\nNothing will content them but the extremest limit of the land; loitering under the shady lee of yonder warehouses will not suffice.\r\nNo.\r\nThey must get just as nigh the water as they possibly can without falling in.\r\n",
          "As they were poor, owing to the amount of milk the children drank, this nurse was a prim Newfoundland dog, called Nana, who had belonged to no one in particular until the Darlings engaged her.\r\nShe had always thought children important, however, and the Darlings had become acquainted with her in Kensington Gardens, where she spent most of her spare time peeping into perambulators, and was much hated by careless nursemaids, whom she followed to their homes and complained of to their mistresses.",
          "Look at the crowds of water-gazers there.\r\nCircumambulate the city of a dreamy Sabbath afternoon.\r\nGo from Corlears Hook to Coenties Slip, and from thence, by Whitehall, northward.\r\nWhat do you see?—Posted like silent sentinels all around the town, stand thousands upon thousands of mortal men fixed in ocean reveries.\r\nSome leaning against the spiles; some seated upon the pier-heads; some looking over the bulwarks of ships from China; some high aloft in the rigging, as if striving to get a still better seaward peep.\r\nBut these are all landsmen; of week days pent up in lath and plaster—tied to counters, nailed to benches, clinched to desks.\r\nHow then is this?",
          "And still deeper the meaning of that story of Narcissus, who because he could not grasp the tormenting, mild image he saw in the fountain, plunged into it and was drowned.\r\nBut that same image, we ourselves see in all rivers and oceans.\r\nIt is the image of the ungraspable phantom of life; and this is the key to it all.\r\nNow, when I say that I am in the habit of going to sea whenever I begin to grow hazy about the eyes, and begin to be over conscious of my lungs, I do not mean to have it inferred that I ever go to sea as a passenger.\r\nFor to go as a passenger you must needs have a purse, and a purse is but a rag unless you have something in it.\r\nBesides, passengers get sea-sick—grow quarrelsome—don't sleep of nights—do not enjoy themselves much, as a general thing;—no, I never go as a passenger; nor, though I am something of a salt, do I ever go to sea as a Commodore, or a Captain, or a Cook.\r\nI abandon the glory and distinction of such offices to those who like them.\r\nFor my part, I abominate all honorable respectable toils, trials, and tribulations of every kind whatsoever.\r\nIt is quite as much as I can do to take care of myself, without taking care of ships, barques, brigs, schooners, and what not.\r\nAnd as for going as cook,—though I confess there is considerable glory in that, a cook being a sort of officer on ship-board—yet, somehow, I never fancied broiling fowls;—though once broiled, judiciously buttered, and judgmatically salted and peppered, there is no one who will speak more respectfully, not to say reverentially, of a broiled fowl than I will.\r\nIt is out of the idolatrous dotings of the old Egyptians upon broiled ibis and roasted river horse, that you see the mummies of those creatures in their huge bake-houses the pyramids.\r\nNo, when I go to sea, I go as a simple sailor, right before the mast, plumb down into the forecastle, aloft there to the royal mast-head.\r\nTrue, they rather order me about some, and make me jump from spar to spar, like a grasshopper in a May meadow.\r\nAnd at first, this sort of thing is unpleasant enough.\r\n",
          "Surely all this is not without meaning."
        ]
      };

      string bodytext = "";

      if (noteWidgets.Notes is null || noteWidgets.Notes.Count == 0)
      {
        localWidgetInfo.CustomState = -1;
        dataJson = JsonSerializer.Serialize(new
        {
          bodytext,
          currentPage = "0",
          pageCount = "0",
          background = Base64ImageEncoder.CreateSolidColorSVG(Colors.Transparent),
          layer = Base64ImageEncoder.CreateSolidColorSVG(ColorHelper.FromArgb(32, 216, 216, 216))
      });
      }
      else
      {
        int a = localWidgetInfo.CustomState; int b = noteWidgets.Notes.Count;
        int index = ((a % b) + b) % b;
        bodytext = noteWidgets.Notes[index];
        localWidgetInfo.CustomState = index;
        dataJson = JsonSerializer.Serialize(new
        {
          bodytext,
          currentPage = (index + 1).ToString(),
          pageCount = b.ToString(),
          background = Base64ImageEncoder.CreateSolidColorSVG(BackgroundColors[index % BackgroundColors.Count]),
          layer = Base64ImageEncoder.CreateSolidColorSVG(ColorHelper.FromArgb(32, 216, 216, 216))
        });
      }

      templateJson = noteWidgetTemplate;
    }

    updateOptions.Template = templateJson;
    updateOptions.Data = dataJson;
    updateOptions.CustomState = localWidgetInfo.CustomState.ToString();

    WidgetManager.GetDefault().UpdateWidget(updateOptions);
  }

  public WidgetProvider()
  {
    var runningWidgets = WidgetManager.GetDefault().GetWidgetInfos();

    foreach (var widgetInfo in runningWidgets)
    {
      var widgetContext = widgetInfo.WidgetContext;
      var widgetId = widgetContext.Id;
      var widgetName = widgetContext.DefinitionId;
      var customState = widgetInfo.CustomState;
      if (!RunningWidgets.ContainsKey(widgetId))
      {
        WidgetInfo runningWidgetInfo = new() { WidgetId = widgetId, WidgetName = widgetName };
        try
        {
          runningWidgetInfo.CustomState = -1;
        }
        catch
        {

        }
        RunningWidgets[widgetId] = runningWidgetInfo;
      }
    }
  }

  private readonly string noteWidgetTemplate = """
    {
        "type": "AdaptiveCard",
        "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.5",
        "body": [
            {
                "type": "TextBlock",
                "text": "${bodytext}",
                "height": "stretch",
                "wrap": true
            },
            {
                "type": "Container",
                "items": [
                    {
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "width": "auto",
                                "items": [
                                    {
                                        "type": "ActionSet",
                                        "actions": [
                                            {
                                                "type": "Action.Execute",
                                                "iconUrl": "https://raw.githubusercontent.com/ZeroFinchNeil/Icons/refs/heads/master/FluentChevronLeft16Regular_White.svg",
                                                "verb": "previous"
                                            }
                                        ]
                                    }
                                ]
                            },
                            {
                                "type": "Column",
                                "width": "stretch",
                                "id": "Action.ExecuteAction.Execute",
                                "items": [
                                    {
                                        "type": "ColumnSet",
                                        "horizontalAlignment": "Center",
                                        "columns": [
                                            {
                                                "type": "Column",
                                                "width": "auto",
                                                "items": [
                                                    {
                                                        "type": "TextBlock",
                                                        "text": "${currentPage}"
                                                    }
                                                ]
                                            },
                                            {
                                                "type": "Column",
                                                "width": "auto",
                                                "items": [
                                                    {
                                                        "type": "TextBlock",
                                                        "text": "/"
                                                    }
                                                ]
                                            },
                                            {
                                                "type": "Column",
                                                "width": "auto",
                                                "items": [
                                                    {
                                                        "type": "TextBlock",
                                                        "text": "${pageCount}"
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                ],
                                "verticalContentAlignment": "Center"
                            },
                            {
                                "type": "Column",
                                "width": "auto",
                                "items": [
                                    {
                                        "type": "ActionSet",
                                        "actions": [
                                            {
                                                "type": "Action.Execute",
                                                "iconUrl": "https://raw.githubusercontent.com/ZeroFinchNeil/Icons/refs/heads/master/FluentChevronRight16Regular_White.svg",
                                                "verb": "next"
                                            }
                                        ]
                                    }
                                ]
                            }
                        ],
                        "style": "default"
                    }
                ],
                "spacing": "None",
                "verticalContentAlignment": "Bottom",
                "backgroundImage": {
                    "url": "${layer}"
                },
                "bleed": true
            }
        ],
        "backgroundImage": {
            "url": "${background}",
            "horizontalAlignment": "Center",
            "verticalAlignment": "Center"
        },
        "speak": "Note widgets"
    }
    """;
}

public class WidgetInfo
{
  public string? WidgetId { get; set; }
  public string? WidgetName { get; set; }
  public int CustomState { get; set; } = -1;
  public bool IsActive { get; set; } = false;
}

//public static class JsonService
//{
//  public static StorageFile? NoteWidgetsJsonFile { get; private set; }
//  private static readonly Mutex _mutex = new(false, """Local\WidgetTest.JsonMutex""");

//  static JsonService()
//  {
//    Initialize();
//  }

//  private static void Initialize()
//  {
//    var localFolder = ApplicationData.Current.LocalFolder;
//    var widgetsFolder = localFolder.CreateFolderAsync("widgets", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
//    NoteWidgetsJsonFile = widgetsFolder.CreateFileAsync("NoteWidgets.json", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
//  }

//  private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

//  public static NoteWidgets? Load()
//  {
//    NoteWidgets? noteWidgets = null;

//    if (NoteWidgetsJsonFile is null)
//      Initialize();

//    try
//    {
//      _mutex.WaitOne();
//      var jsonString = File.ReadAllText(NoteWidgetsJsonFile!.Path);
//      noteWidgets = JsonSerializer.Deserialize<NoteWidgets>(jsonString, _options);
//    }
//    catch
//    {

//    }
//    finally
//    {
//      _mutex.ReleaseMutex();
//    }

//    return noteWidgets;
//  }

//  public static void Save(NoteWidgets noteWidgets)
//  {
//    if (NoteWidgetsJsonFile is null)
//      Initialize();

//    try
//    {
//      _mutex.WaitOne();
//      File.WriteAllText(NoteWidgetsJsonFile!.Path, JsonSerializer.Serialize(noteWidgets, _options));
//    }
//    catch (Exception e)
//    {
//      Debug.WriteLine(e.Message);
//    }
//    finally
//    {
//      _mutex.ReleaseMutex();

//    }
//  }
//}

public class NoteWidgets
{
  public List<string>? Notes { get; set; }
}