using UnityEngine;

namespace RPM_Parts_Checklist
{
    public delegate List<string> DisplayListElement_DisplayDelegate(int index, bool selected, int screenWidth);
    public delegate void ButtonProcessorDelegate(int button);

    public class DisplayListElement
    {
        public DisplayListElement_DisplayDelegate Display;
        public ButtonProcessorDelegate ButtonProcessor;

        public DisplayListElement(DisplayListElement_DisplayDelegate Display, ButtonProcessorDelegate ButtonProcessor)
        {
            this.Display = Display;
            this.ButtonProcessor = ButtonProcessor;
        }
    }

    public class DisplayList
    {
        public List<DisplayListElement> elements = new();
        public int index = 0;
        int windowTop = 0;
        int windowSize = 0;

        public string Display(int screenWidth, int windowSize)
        {
            this.windowSize = windowSize;
            string output = "";
            List<string> lines = new();

            for (int i = 0; i < elements.Count; i++) lines.AddRange(elements[i].Display(i, i == index, screenWidth));

            for (int i = windowTop; i < (windowTop + windowSize) && i < lines.Count; i++) output += lines[i] + Environment.NewLine;

            if (lines.Count > windowTop + windowSize) output += "...";

            return output;
        }

        public void ScrollUp()
        {
            if (index == 0) index = elements.Count - 1;
            else index--;

            while (index < windowTop) windowTop--;
            while (index >= (windowTop + windowSize)) windowTop++;
        }

        public void ScrollDown()
        {
            index++;
            if (index == elements.Count) index = 0;

            while (index < windowTop) windowTop--;
            while (index >= (windowTop + windowSize)) windowTop++;
        }

        public void ButtonProcessor(int button)
        {
            if (button == 0) ScrollUp();
            else if (button == 1) ScrollDown();
            else
            {
                if (index < elements.Count)
                {
                    elements[index].ButtonProcessor(button);
                }
            }
        }
    }

    public delegate string Page_DisplayDelegate(int screenWidth, int screenHeight);
    public delegate void Page_ButtonProcessorDelegate(int button);
    public class Page
    {
        public string name;
        public Page_DisplayDelegate Display;
        public Page_ButtonProcessorDelegate ButtonProcessor;

        public Page(string name, Page_DisplayDelegate Display, Page_ButtonProcessorDelegate ButtonProcessor)
        {
            this.name = name;
            this.Display = Display;
            this.ButtonProcessor = ButtonProcessor;
        }
    }

    public class Router
    {
        public List<Page> pages = new();

        List<string> pageStack = new();

        public Router(string firstPage)
        {
            pageStack.Add(firstPage);
        }
        public string Display(int screenWidth, int screenHeight)
        {
            foreach (Page page in pages)
            {
                if (page.name == pageStack[pageStack.Count - 1])
                {
                    return page.Display(screenWidth, screenHeight);
                }
            }

            return "";
        }

        public void Navigate(string page, bool addToStack = true, bool previous = false)
        {
            if (previous)
            {
                if (pageStack.Count > 1) pageStack.RemoveAt(pageStack.Count - 1);
            }
            else
            {
                if (!addToStack) pageStack.Clear();
                pageStack.Add(page);
            }
        }

        public void ButtonProcessor(int button)
        {
            if (button == 4)
            {
                Navigate("", false, true);
            } else
            {
                foreach (Page page in pages)
                {
                    if (page.name == pageStack[pageStack.Count - 1])
                    {
                        page.ButtonProcessor(button);
                        break;
                    }
                }
            }
        }
    }
}
