namespace StorageFilterIcons;

// All user-visible text. Translations are loaded from translations/<language code>.po (see StorageFilterIconsMod).
public static class STRINGS
{
    public static class OPTIONS
    {
        public static class OPACITY
        {
            public static LocString NAME = "Icon opacity";
            public static LocString TOOLTIP = "Opacity of the overlay icons (when not hovered, or when hover opacity is Translucent).";
        }

        public static class HOVER_SCALE
        {
            public static LocString NAME = "Icons on hover: size";
            public static LocString TOOLTIP = "What happens to the overlay icons while the cursor is over the building.";
            public static LocString ENLARGE = "Enlarge";
            public static LocString ENLARGE_TOOLTIP = "Enlarge the icons.";
            public static LocString NORMAL = "Unchanged";
            public static LocString NORMAL_TOOLTIP = "Keep the icons at their normal size.";
            public static LocString HIDE = "Hide";
            public static LocString HIDE_TOOLTIP = "Hide the icons.";
        }

        public static class HOVER_OPACITY
        {
            public static LocString NAME = "Icons on hover: opacity";
            public static LocString TOOLTIP = "Opacity of the overlay icons while the cursor is over the building. Ignored when the size option is Hide.";
            public static LocString TRANSLUCENT = "Translucent";
            public static LocString TRANSLUCENT_TOOLTIP = "Keep the icons translucent.";
            public static LocString OPAQUE = "Opaque";
            public static LocString OPAQUE_TOOLTIP = "Draw the icons fully opaque.";
        }

        public static class SHOW_IN_HOVER_CARD
        {
            public static LocString NAME = "Show icons in hover card";
            public static LocString TOOLTIP = "Also list the filtered resource icons in the card shown when hovering a building.";
        }

        public static class DISABLED_BUILDINGS
        {
            public static LocString NAME = "Buildings";
            public static LocString TOOLTIP = "Choose the buildings that show filter icons.";
        }
    }
}
