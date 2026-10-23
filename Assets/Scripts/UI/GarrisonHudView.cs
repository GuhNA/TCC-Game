using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.UI
{
    public sealed class GarrisonHudView : MonoBehaviour
    {
        [Serializable]
        private sealed class UnitRow
        {
            public TMP_Text stationed;
            public TMP_Text reserve;
            public Button receiveOne;
            public Button receiveTen;
            public Button sendOne;
            public Button sendTen;
        }

        [SerializeField] private TMP_Text health;
        [SerializeField] private TMP_Text threat;
        [SerializeField] private TMP_Text countdown;
        [SerializeField] private UnitRow[] rows;

        public void SetStrategicState(int currentHealth, int maximum, string threatText, string time)
        {
            health.text = $"{currentHealth} / {maximum}";
            threat.text = string.IsNullOrEmpty(threatText) ? "—" : threatText.Replace("Ameaça ", string.Empty);
            countdown.text = string.IsNullOrEmpty(time) ? "Sem onda" : time;
        }

        public void SetUnit(int index, int stationed, int reserve, bool active)
        {
            UnitRow row = rows[index];
            row.stationed.text = stationed.ToString();
            row.reserve.text = reserve.ToString();
            row.receiveOne.interactable = active && reserve >= 1;
            row.receiveTen.interactable = active && reserve >= 10;
            row.sendOne.interactable = active && stationed >= 1;
            row.sendTen.interactable = active && stationed >= 10;
        }
    }
}
