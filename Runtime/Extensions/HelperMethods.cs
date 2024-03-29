using UnityEngine;
using System.Collections.Generic;
using System;
namespace Intelmatix
{
    public static class Helpers
    {
        public static void DestroyChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        public static void DestroyChildren(this RectTransform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public static LTDescr BlockCanvas(this CanvasGroup canvasGroup, float duration = 0)//, bool interactable, bool blockRaycast)
        {
            Debug.Log("blocking raycast");
            canvasGroup.blocksRaycasts = true;
            return canvasGroup.LeanAlpha(1, duration);//.setOnStart(() => canvasGroup.blocksRaycasts = true);
        }

        public static LTDescr UnblockCanvas(this CanvasGroup canvasGroup, float duration = 0)//, bool interactable, bool blockRaycast)
        {
            Debug.Log("unblocking raycast");
            //canvasGroup.blocksRaycasts = false;
            return canvasGroup.LeanAlpha(0, duration).setOnComplete(() => canvasGroup.blocksRaycasts = false);
        }


        public static string GetNumberConversion(this float number)
        {
            var isPositive = number >= 0;
            var absNumber = Math.Abs(number);
            var suffix = isPositive ? "" : "-";
            if (absNumber >= 1000000000) // use B
                return suffix + (absNumber / 1000000000).ToString("0.0") + "B";
            else if (absNumber >= 1000000) // use M
                return suffix + (absNumber / 1000000).ToString("0.0") + "M";
            else if (absNumber >= 1000) // use K
                return suffix + (absNumber / 1000).ToString("0.0") + "K";
            else
                return suffix + absNumber.ToString("0.0");
        }

        public static void SetPositionY(this LineRenderer lineRenderer, float y, int index = 1)
        {
            var line = lineRenderer.GetPosition(index);
            lineRenderer.SetPosition(index, new Vector3(line.x, y, line.z));
        }
        public static float GetPositionY(this LineRenderer lineRenderer, int index = 1)
        {
            var line = lineRenderer.GetPosition(index);
            return line.y;
        }

        public static void SetLabelOpacity(this AwesomeCharts.LineChart lineChart, float opacity)
        {
            var labelConfig = lineChart.AxisConfig.HorizontalAxisConfig.LabelsConfig;
            labelConfig.LabelColor = new Color(1, 1, 1, opacity);
            if (lineChart.AxisConfig.HorizontalAxisConfig.LabelsCount < 12)
            {
                lineChart.AxisConfig.HorizontalAxisConfig.LabelsConfig.LabelColor = new Color(1, 1, 1, 1);
            }
        }

        public static void SetLabelOpacity(this AwesomeCharts.BarChart barChart, float opacity)
        {
            var labelConfig = barChart.AxisConfig.HorizontalAxisConfig.LabelsConfig;
            labelConfig.LabelColor = new Color(1, 1, 1, opacity);
            if (barChart.GetChartData().DataSets.Count > 0 && barChart.GetChartData().DataSets[0].Entries.Count < 7)
            {
                barChart.AxisConfig.HorizontalAxisConfig.LabelsConfig.LabelColor = new Color(1, 1, 1, 1);
            }
        }

        public static Vector3 GetWorldPosition(this RectTransform rectTransform, Camera camera)
        {
            // Obtener las esquinas del rectángulo local
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            // Obtener el centro del rectángulo en el espacio local
            Vector3 center = rectTransform.rect.center;

            // Promedio de las esquinas para obtener el centro del rectángulo en el espacio mundial
            Vector3 worldCenter = Vector3.zero;
            for (int i = 0; i < corners.Length; i++)
            {
                worldCenter += corners[i];
            }
            worldCenter /= corners.Length;

            // Obtener la distancia desde la cámara al rectángulo
            float distance = Vector3.Distance(camera.transform.position, worldCenter);

            // Proyectar el centro del rectángulo en el espacio mundial hacia la cámara
            Vector3 worldPosition = camera.transform.position + camera.transform.forward * distance;

            return worldPosition;
        }
    }

}