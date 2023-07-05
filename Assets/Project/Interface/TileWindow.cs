using System;
using System.Linq;
using CellEngine.World;
using JUtils.UI;
using UnityEngine;
using UnityEngine.UIElements;



namespace CellEngine.Interface
{
    public class TileWindow : UIWindow<TileWindow>
    {
        [SerializeField] private World.CellEngine _cellEngine;
        [SerializeField] private TileSpawner      _spawner;
        
        private TileList _tileList;


        protected override void Awake()
        {
            base.Awake();
            Activate();
        }


        protected override void OnInitialize()
        {
            _tileList = AddUIElement<TileList>("List");
            _tileList.onClick += HandleClick;
        }


        protected override void OnActivate()
        {
            _tileList.SetData(_cellEngine.cellTemplates);
            _tileList.onClick?.Invoke(_cellEngine.cellTemplates.First());
        }


        protected override void OnDeactivate()
        {
        }


        private void HandleClick(CellTemplate cellTemplate)
        {
            _spawner.activeTemplate = cellTemplate;
        }


        public class TileList : UIList<CellTemplate>
        {
            public Action<CellTemplate> onClick;
            
            
            protected override void OnInitialize()
            {
                listElement = AddUIElement<TileListElement>();
            }
        }



        public class TileListElement : UIListElement<CellTemplate>
        {
            protected override string   defaultQuery => "TileButton";
            protected new      TileList parent       => base.parent as TileList;
            

            protected override void OnInitialize()
            {
                parent.onClick += HandleClick;
            }


            protected override void OnActivate(CellTemplate data)
            {
                Button btn = element.Q<Button>();
                btn.style.backgroundColor =  data.color;
                btn.Q<Button>().clicked   += () => parent.onClick?.Invoke(data);
            }


            private void HandleClick(CellTemplate template)
            {
                if (!active) return;
                Button btn = element.Q<Button>();
                btn.RemoveFromClassList("selected");

                if (Equals(template, data)) {
                    btn.AddToClassList("selected");
                }
            }
        }
    }
}
