using KSP.Localization;

namespace RPM_Parts_Checklist
{
    delegate void SetSelectedPartDelegate(Part part);
    public class RPM_PartList : InternalModule
    {
        [KSPField]
        public string pageTitle = "============== Part List ===============";
        [KSPField]
        public int buttonUp = 0;
        [KSPField]
        public int buttonDown = 1;
        [KSPField]
        public int buttonEnter = 2;
        [KSPField]
        public int buttonEsc = 3;
        [KSPField]
        public int buttonHome = 4;
        [KSPField]
        public int buttonRight = 5;
        [KSPField]
        public int buttonLeft = 6;
        [KSPField]
        public int buttonNext = 7;
        [KSPField]
        public int buttonPrev = 8;

        Router router;

        Part? selectedPart = null;

        public RPM_PartList()
        {
            router = new("home");

            Parts_HomeView homeView = new Parts_HomeView(pageTitle, router);
            router.pages.Add(new Page("home", homeView.Display, homeView.ButtonProcessor));

            Parts_HelpView helpView = new Parts_HelpView(pageTitle);
            router.pages.Add(new Page("help", helpView.Display, helpView.ButtonProcessor));

            Parts_PartListView partListView = new Parts_PartListView(pageTitle);
            router.pages.Add(new Page("partList", (int screenWidth, int screenHeight) => partListView.Display(screenWidth, screenHeight, (Part part) => { selectedPart = part; }, router), partListView.ButtonProcessor));

            Parts_PartView partView = new Parts_PartView(pageTitle);
            router.pages.Add(new Page("part", (int screenWidth, int screenHeight) => partView.Display(screenWidth, screenHeight, selectedPart), partView.ButtonProcessor));
        }
        public string Display(int screenWidth, int screenHeight)
        {
            return router.Display(screenWidth, screenHeight);
        }

        public void ButtonProcessor(int button)
        {
            router.ButtonProcessor(button);
        }
    }

    class Parts_HomeView
    {
        DisplayList dl;
        string pageTitle;
        public Parts_HomeView(string pageTitle, Router router)
        {
            this.pageTitle = pageTitle;
            dl = new();

            //Part list
            dl.elements.Add(new DisplayListElement(
                (int index, bool selected, int screenWidth) =>
                {
                    return new List<string>() { (selected ? "[#20BF6BFF]" : "") + "Show part list" };
                },
                (int button) => {
                    if (button == 2) router.Navigate("partList");
                }
            ));

            //Help
            dl.elements.Add(new DisplayListElement(
                (int index, bool selected, int screenWidth) =>
                {
                    return new List<string>() { (selected ? "[#20BF6BFF]" : "") + "Help" };
                },
                (int button) => {
                    if (button == 2) router.Navigate("help");
                }
            ));
        }

        public string Display(int screenWidth, int screenHeight)
        {
            return Environment.NewLine + pageTitle + Environment.NewLine + Environment.NewLine + dl.Display(screenWidth, screenHeight - 5);
        }

        public void ButtonProcessor(int button)
        {
            dl.ButtonProcessor(button);
        }
    }

    class Parts_HelpView
    {
        string pageTitle;

        public Parts_HelpView(string pageTitle)
        {
            this.pageTitle = pageTitle;
        }
        public string Display(int screenWidth, int screenHeight)
        {
            return Environment.NewLine +
                pageTitle + Environment.NewLine +
                "Reduce by 1%                   Cursor up" + Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine +
                "Increase by 1%               Cursor down" + Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine +
                "Increase by 10%          Select / Enable" + Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine +
                "Decrease by 10%                  Disable" + Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine +
                "                                  Return" + Environment.NewLine +
                "Version: 1.1.0";
        }

        public void ButtonProcessor(int button)
        {

        }
    }

    class Parts_PartListView
    {
        DisplayList dl = new();
        string pageTitle;

        public Parts_PartListView(string pageTitle)
        {
            this.pageTitle = pageTitle;
        }

        List<Part> GetPartsWithOptions()
        {
            List<Part> parts = new();

            foreach (Part part in FlightGlobals.ActiveVessel.parts)
            {
                bool append = false;
                foreach (PartModule pm in part.Modules)
                {
                    if (pm.Fields != null)
                    {
                        foreach (BaseField f in pm.Fields)
                        {
                            if (f.guiActive && f.guiName != "")
                            {
                                string type = f.uiControlFlight.GetType().ToString();
                                if (type == "UI_Toggle" || type == "UI_FloatRange" || type == "UI_Cycle" || type == "UI_Label") append = true;
                            }
                        }
                    }

                    if (pm.Events != null)
                    {
                        foreach (BaseEvent ev in pm.Events)
                        {
                            if (ev.active && ev.guiActive) append = true;
                        }
                    }
                }

                if (append) parts.Add(part);
            }

            return parts;
        }

        public string Display(int screenWidth, int screenHeight, SetSelectedPartDelegate SetSelectedPart, Router router)
        {
            dl.elements.Clear();

            List<Part> parts = GetPartsWithOptions();
            foreach (Part part in parts)
            {
                dl.elements.Add(new DisplayListElement(
                    (int index, bool selected, int screenWidth) =>
                    {
                        return new List<string>() { (selected ? "[#20BF6BFF]" : "") + part.partInfo.title };
                    },
                    (int button) => {
                        if (button == 2)
                        {
                            SetSelectedPart(part);
                            router.Navigate("part");
                        }
                    }
                ));
            }

            if (dl.index >= dl.elements.Count) dl.index = dl.elements.Count - 1;

            return Environment.NewLine + pageTitle + Environment.NewLine + Environment.NewLine + dl.Display(screenWidth, screenHeight - 5);
        }

        public void ButtonProcessor(int button)
        {
            dl.ButtonProcessor(button);
        }
    }

    class Parts_PartView
    {
        DisplayList dl = new();
        string pageTitle;
        public Parts_PartView(string pageTitle)
        {
            this.pageTitle = pageTitle;
        }

        List<object> GetPartElements(Part p)
        {
            List<object> elements = new();

            foreach (PartModule pm in p.Modules)
            {
                if (pm.Fields != null)
                {
                    foreach (BaseField field in pm.Fields)
                    {
                        if (field.guiActive && field.guiName != "")
                        {
                            string type = field.uiControlFlight.GetType().ToString();
                            if (type == "UI_Toggle" || type == "UI_FloatRange" || type == "UI_Cycle" || type == "UI_Label") elements.Add(field);
                        }
                    }
                }

                if (pm.Events != null)
                {
                    foreach (BaseEvent ev in pm.Events)
                    {
                        if (ev.active && ev.guiActive) elements.Add(ev);
                    }
                }
            }

            return elements;
        }

        void UpdatePropertyValue(BaseField f, string action)
        {
            string type = f.uiControlFlight.GetType().ToString();
            switch (type)
            {
                case "UI_Toggle":
                    bool bValue = f.GetValue<bool>(f.host);

                    if (action == "enter") bValue = true;
                    if (action == "esc") bValue = false;

                    f.SetValue(bValue, f.host);
                    break;

                case "UI_FloatRange":
                    float fValue = f.GetValue<float>(f.host);
                    UI_FloatRange floatRange = (UI_FloatRange)f.uiControlFlight;
                    float onePercent = (floatRange.maxValue - floatRange.minValue) * 0.01f;

                    if (action == "right") fValue += onePercent;
                    if (action == "left") fValue -= onePercent;
                    if (action == "next") fValue += onePercent * 10;
                    if (action == "prev") fValue -= onePercent * 10;

                    if (action == "enter") fValue = floatRange.maxValue;
                    if (action == "esc") fValue = floatRange.minValue;

                    fValue = Math.Min(Math.Max(fValue, floatRange.minValue), floatRange.maxValue);
                    f.SetValue(fValue, f.host);
                    break;

                case "UI_Cycle":
                    int iValue = f.GetValue<int>(f.host);
                    UI_Cycle cycle = (UI_Cycle)f.uiControlFlight;

                    if (action == "right" || action == "next" || action == "enter") iValue++;
                    if (action == "left" || action == "prev") iValue--;

                    if (iValue < 0) iValue = cycle.stateNames.Count() - 1;
                    if (iValue >= cycle.stateNames.Count()) iValue = 0;

                    f.SetValue(iValue, f.host);
                    break;
            }
        }

        public string Display(int screenWidth, int screenHeight, Part selectedPart)
        {
            dl.elements.Clear();
            List<object> selectedPartElements = GetPartElements(selectedPart);

            foreach (object ob in selectedPartElements)
            {
                dl.elements.Add(new DisplayListElement(
                    (int index, bool selected, int screenWidth) =>
                    {
                        List<string> output = new();
                        string line = "";

                        if (ob is BaseField)
                        {
                            BaseField field = (BaseField)ob;

                            string type = field.uiControlFlight.GetType().ToString();
                            string left = field.guiName;
                            string right = "";
                            if (type == "UI_Toggle")
                            {
                                bool value = field.GetValue<bool>(field.host);
                                UI_Toggle toggle = (UI_Toggle)field.uiControlFlight;
                                if (value) right += "[#20BF6BFF]" + toggle.displayEnabledText + "[#FFFFFFFF]";
                                else right += "[#EB3B5AFF]" + toggle.displayDisabledText + "[#FFFFFFFF]";

                                while (left.Length + right.Length - 22 < screenWidth) left += " ";
                            }
                            else if (type == "UI_FloatRange")
                            {

                                float minValue = ((UI_FloatRange)field.uiControlFlight).minValue;
                                float maxValue = ((UI_FloatRange)field.uiControlFlight).maxValue;

                                if (minValue == 0 && maxValue == 100) right = field.GetStringValue(field.host, true) + "%";
                                else right = field.GetStringValue(field.host, true) + " (" + minValue + ", " + maxValue + ")";

                                while (left.Length + right.Length < screenWidth) left += " ";
                            }
                            else if (type == "UI_Cycle")
                            {
                                string value = Localizer.Format(((UI_Cycle)field.uiControlFlight).stateNames[(int)field.GetValue(field.host)]);
                                right = value;

                                while (left.Length + right.Length < screenWidth) left += " ";
                            }
                            else if (type == "UI_Label")
                            {
                                right = field.GetStringValue(field.host, true);   
                                while (left.Length + right.Length < screenWidth) left += " ";
                            }

                            string textColor = type == "UI_Label" ? "[#AAAAAAFF]" : "[#FFFFFFFF]";
                            line = (selected ? "[#20BF6BFF]" : textColor) + left + textColor + right;
                        }
                        else if (ob is BaseEvent)
                        {
                            BaseEvent ev = (BaseEvent)ob;
                            line = (selected ? "[#20BF6BFF]" : "[#FFFFFFFF]") + ev.guiName;
                        }
                        output.Add(line);
                        return output;
                    },
                    (int button) =>
                    {
                        if (button == 2)
                        {
                            //Enter
                            if (ob is BaseField) UpdatePropertyValue((BaseField)ob, "enter");
                            else if (ob is BaseEvent) ((BaseEvent)ob).Invoke();
                        }

                        if (button == 3)
                        {
                            //Esc
                            if (ob is BaseField) UpdatePropertyValue((BaseField)ob, "esc");
                        }

                        if (button == 5)
                        {
                            //Right
                            if (ob is BaseField) UpdatePropertyValue((BaseField)ob, "right");
                        }

                        if (button == 6)
                        {
                            //Left
                            if (ob is BaseField) UpdatePropertyValue((BaseField)ob, "left");
                        }

                        if (button == 7)
                        {
                            //Next
                            if (ob is BaseField) UpdatePropertyValue((BaseField)ob, "next");
                        }

                        if (button == 8)
                        {
                            //Prev
                            if (ob is BaseField) UpdatePropertyValue((BaseField)ob, "prev");
                        }
                    }
                ));
            }
            
            if (dl.index >= dl.elements.Count) dl.index = dl.elements.Count - 1;

            return Environment.NewLine + pageTitle + Environment.NewLine + selectedPart.partInfo.title + Environment.NewLine + dl.Display(screenWidth, screenHeight - 5);
        }

        public void ButtonProcessor(int button)
        {
            dl.ButtonProcessor(button);
        }
    }
}
