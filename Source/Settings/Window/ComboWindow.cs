using System;
using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
    public class ComboWindow : Window
    {
        public const string HEADER_TIP =
            "Hover over the header for the description.";

        public Action<int> callback = null;
        public string header = "";
        public string description = "";
        public string[] texts = Array.Empty<string>();

        public Vector2 scrollVector = Vector2.zero;
        public float scrollHeight = 0f;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(320f, 320f);
            }
        }

        public ComboWindow(Action<int> callback, string header, string description, params string[] texts)
        {
            this.callback = callback;
            this.header = header;
            this.description = description;
            this.texts = texts;

            doCloseButton = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            forcePause = true;

            Find.WindowStack.Add(this);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard gui = new Listing_Standard
            {
                maxOneColumn = true
            };

            gui.Begin(inRect);
            {
                Text.Font = GameFont.Tiny;
                {
                    gui.Label(HEADER_TIP);
                }
                Text.Anchor = TextAnchor.UpperCenter;
                Text.Font = GameFont.Medium;
                {
                    gui.Label(header, tooltip: description);
                }
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;

                gui.Gap(20f);

                float height = gui.CurHeight;

                Widgets.BeginScrollView(
                    new Rect(
                        0f,
                        height,
                        gui.ColumnWidth,
                        inRect.height - height - 40f
                    ),
                    ref scrollVector,
                    new Rect(
                        0f,
                        height,
                        gui.ColumnWidth - 16f,
                        scrollHeight
                    )
                );
                {
                    for (int i = 0; i < texts.Length; i++)
                    {
                        if (gui.ButtonText(texts[i]))
                        {
                            callback?.Invoke(i);
                            Close();
                        }
                    }

                    scrollHeight = gui.CurHeight - height;
                }
                Widgets.EndScrollView();
            }
            gui.End();
        }

        public static void Entry(Listing_Standard gui,
                                 Settings.State state,
                                 string key,
                                 string header,
                                 string description,
                                 params string[] texts)
        {
            int n = state.Get(key);

            if (gui.ButtonTextLabeled(header, texts[n]))
                new ComboWindow(i => state.Set(key, i), header, description, texts);
        }
    }
}
