using UnityEngine;


namespace StorageFilterIcons;

// Finds the building under the mouse cursor once per frame; shared by all overlays.
internal static class HoverTracker
{
    private static int frame = -1;
    private static GameObject current;

    public static GameObject Current
    {
        get
        {
            if (frame != Time.frameCount)
            {
                frame = Time.frameCount;
                current = Find();
            }

            return current;
        }
    }

    private static GameObject Find()
    {
        var camera = Camera.main;
        if (camera == null || Grid.WidthInCells <= 0)
        {
            return null;
        }

        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return null;
        }

        var mouse = Input.mousePosition;
        mouse.z = -camera.transform.GetPosition().z;
        var cell = Grid.PosToCell(camera.ScreenToWorldPoint(mouse));
        if (!Grid.IsValidCell(cell))
        {
            return null;
        }

        return Grid.Objects[cell, (int)ObjectLayer.Building];
    }
}
