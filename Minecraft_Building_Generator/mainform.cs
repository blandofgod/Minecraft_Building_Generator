﻿using Minecraft_Building_Generator.Command_Generator;
using Minecraft_Building_Generator.Grid_Classes;
using Minecraft_Building_Generator.Structures;
using Minecraft_Building_Generator.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Minecraft_Building_Generator
{
    public partial class mainform : Form
    {
        GridMap aMap { get; set; } //Complete layout of the grid
        UI_Grid_Planning_Container[,] GridPlanner_Map { get; set; } //handles UI grid layout



        //List<UI_Grid_Planning_Rectangle> gridPlanner { get; set; }
        //List<UI_Grid_Planning_Container> gridsqarePlanner { get; set; }
        //Graphics gridcontainer_planning_graphic { get; set; }
        //Graphics gridsquare_planning_graphic { get; set; }


        /*UI Panels*/
        UI_GridPanel gridcontainerPanel { get; set; }
        UI_GridPanel gridsquarePanel { get; set; }
        
        /*Important Variables that track states of the application for comparison*/
        private UI_Grid_Planning_Container selected_container { get; set; }
        private UI_Grid_Planning_Container previouslySelected { get; set; }
        private GridSquare_Zoning selected_radioButton_gridzone { get; set; }
        private int perviousSizeOfGrid { get; set; }

        /*Used for Drawing on the UI Grid - sizes vary due to potential dynamic shift*/
        private int UI_squares_draw_Direction_Max_X { get; set; }
        private int UI_squares_draw_Direction_Max_Y { get; set; }
        private int UI_Container_draw_Direction_Max_X { get; set; }
        private int UI_Container_draw_Direction_Max_Y { get; set; }



        public mainform()
        {

            InitializeComponent();
            Initialize_CityGenerator_Form();
            
            //Size = new Size(500, 500);
        }

        private void Initialize_CityGenerator_Form()
        {
            //These initialize grid_panels which fire off Paint Methods starting the UI Building
            gridcontainerPanel = new UI_GridPanel(panel_grid_planning, panel_grid_planning.CreateGraphics());
            gridsquarePanel = new UI_GridPanel(panel_grid_square_planning, panel_grid_square_planning.CreateGraphics());

            /*Starting coordinates default*/
            textbox_startcoordinate_x.Text = "0";
            textbox_startcoordinate_y.Text = "0";
            textbox_startcoordinate_z.Text = "0";

            /*Set Default size*/
            comboBox_how_large.SelectedIndex = 0;

            //Creates wordwrap for description label
            label_behaviorpack_info.MaximumSize = new Size(500, 0);
            label_behaviorpack_info.AutoSize = true;

            /*Initialize the windows form*/
            textBox_tab_minecraftversion_1.Text = "1";
            textBox_tab_minecraftversion_2.Text = "15";
            textBox_tab_minecraftversion_3.Text = "0";

            /*UUID*/
            textBox_tab_UUID.Text = "abe07dbe - 3461 - 11eb - adc1 - 0242ac120002";

            /*Description*/
            textBox_tab_behaviorpack_description.Text = "A customized city generated";

            /*Version*/
            textBox_behaviorpack_version.Text = "1.0";

            //gridPlanner = new List<UI_Grid_Planning_Rectangle>();



           
        }


        private void comboBox_how_large_SelectedIndexChanged(object sender, EventArgs e)
        {
            //reset grids
            if(GridPlanner_Map != null)
                Redraw_Grid_Map(gridcontainerPanel, gridsquarePanel);

            string selected = comboBox_how_large.SelectedItem.ToString();
            int selectedSize = int.Parse(selected);

            int sizeOfBox = 169 * GridSize();
            int TotalSize = sizeOfBox * sizeOfBox;

            dynamic_label_generated_size.Text = $"{sizeOfBox} x {sizeOfBox} = {TotalSize} blocks^2";

            aMap = new GridMap(
                int.Parse(textbox_startcoordinate_x.Text),
                int.Parse(textbox_startcoordinate_y.Text),
                int.Parse(textbox_startcoordinate_z.Text),
                selectedSize);

            aMap.GenerateGrids();



        }


        /**
         * Button handling methods
         */
        private void button_generate_Click(object sender, EventArgs e)
        {
            label_export_complete.Text = "";
            label_export_complete.Refresh();

            Generate_Commands gc = new Generate_Commands();
            Build_Manager bm = new Build_Manager(aMap.PrimaryGridMap);
            bm.Process_Containers();
            string status = gc.ExportCommandstoFiles(aMap.PrimaryGridMap);

            label_export_complete.Text = status;
        }



        /**
         * Panel Paint Methods
         */
        private void panel_grid_planning_Paint(object sender, PaintEventArgs e)
        {
            Draw_UI_Grid_Containers(GridSize());

            //initialization assumes 0,0.  this is okay because loading state method will fill GridPlanner_Map before a grid is painted.
            Draw_UI_Grid_Squares(); 
        }
      
        private void panel_grid_square_planning_Paint(object sender, PaintEventArgs e)
        {
            //empty because this is dependant on Container click events
        }




        /**
         * Grid Container Methods
         */


        /// <summary>
        /// Initializes the UI Grid Container section. This is called on every Paint call
        /// </summary>
        /// <param name="sizeOfGrid"></param>
        /// <param name="UI_Grid_Containers"></param>
        private void Draw_UI_Grid_Containers(int sizeOfGrid)
        {
            Console.WriteLine("initilize container");
            //Grid_production
            int separatorValue = 17;
            int x = 10;
            int y = 10;
            int maxX = 1;
            int maxY = 1;

            //initialize 2d array of grid planner
            GridPlanner_Map = new UI_Grid_Planning_Container[sizeOfGrid, sizeOfGrid];

            for (int i = 0; i < sizeOfGrid; i++)
            {
                for (int j = 0; j < sizeOfGrid; j++)
                {

                    Rectangle _rect = new Rectangle(x, y, Shared_Constants.UI_GRID_RECTANGLE_SIZE, Shared_Constants.UI_GRID_RECTANGLE_SIZE);
                    GridPlanner_Map[i, j] = new UI_Grid_Planning_Container(_rect);
                    gridcontainerPanel.Fill_Rectangle(GridSquare_Zoning.Initialized, _rect);

                    x += separatorValue;
                }
                maxX = x;
                maxY = y + separatorValue + 10;
                x = 10;
                y += separatorValue;
            }


            UI_Container_draw_Direction_Max_X = maxX;
            UI_Container_draw_Direction_Max_Y = maxY;

            UI_Draw_Grid_Support(gridcontainerPanel, UI_Container_draw_Direction_Max_X, UI_Container_draw_Direction_Max_Y);


            Point p1 = new Point(maxX + 5, 0);
            UI_Draw_Grid_Support("East", gridcontainerPanel, p1);
            Point p2 = new Point(0, maxY + 5);
            UI_Draw_Grid_Support("South", gridcontainerPanel, p2);

            //initializes the first continer to be selected.
            GridPlanner_Map[0, 0] = Set_Select_UI_Grid_Planning_Container(GridPlanner_Map[0, 0]);
            selected_container = GridPlanner_Map[0, 0];


        }


        /// <summary>
        /// This method handles what action will be taken when a container is selected
        /// </summary>
        /// <param name="SizeOfGrid"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private UI_Grid_Planning_Container Select_Rectangle_Container(int SizeOfGrid, Point location)
        {

            UI_Grid_Planning_Container rectangle;
            for (int i = 0; i < SizeOfGrid; i++)
            {
                for (int j = 0; j < SizeOfGrid; j++)
                {
                    rectangle = GridPlanner_Map[i, j];

                    if (rectangle.rect.Contains(location))
                    {
                        if (previouslySelected == rectangle)
                        {
                            Console.WriteLine("Match");
                            Set_Select_UI_Grid_Planning_Container(rectangle);
                        }
                        else
                        {
                            rectangle = Set_Select_UI_Grid_Planning_Container(rectangle);
                           
                        }

                        return rectangle;
                    }
                }

            }
            return null;
        }





        /**
         * Mouse Click Handling Methods
         */

        /// <summary>
        /// Action taken when a Grid_container is clicked
        /// </summary>
        private void panel_grid_planning_MouseClick(Object sender, MouseEventArgs e)
        {

            UI_Grid_Planning_Container aContainer = Select_Rectangle_Container(GridSize(), e.Location);
            if (aContainer != null)
            {
                selected_container = aContainer;
                gridsquarePanel.gridPanel.Refresh();
                Load_Grid_Squares(selected_container);
            }

        }


        /**
         * Grid Square Methods
         */


        /// <summary>
        /// Initializes Grid squares in the UI Grid Map
        /// </summary>
        private void Draw_UI_Grid_Squares()
        {

            UI_Grid_Planning_Container selected_container;
            Console.WriteLine("initilize squares");
            //Grid_production
            int separatorValue = 17;
            int x = 10;
            int y = 10;
            int maxX = 1;
            int maxY = 1;

            //initialize 2d array of grid planner

            for (int i = 0; i < GridSize(); i++)
            {
                for (int j = 0; j < GridSize(); j++)
                {
                    selected_container = GridPlanner_Map[i, j];

                    UI_Grid_Planning_Square[,] _uiGridSquares = new UI_Grid_Planning_Square[Shared_Constants.GRID_SQUARE_SIZE, Shared_Constants.GRID_SQUARE_SIZE];

                    for (int m = 0; m < Shared_Constants.GRID_SQUARE_SIZE; m++)
                    {
                        for (int n = 0; n < Shared_Constants.GRID_SQUARE_SIZE; n++)
                        {
                            Rectangle _rect = new Rectangle(x, y, Shared_Constants.UI_GRID_RECTANGLE_SIZE, Shared_Constants.UI_GRID_RECTANGLE_SIZE);
                            _uiGridSquares[m, n] = new UI_Grid_Planning_Square(_rect);
                            if(i == 0 && j ==0)
                                gridsquarePanel.Fill_Rectangle(GridSquare_Zoning.Initialized, _rect);

                            x += separatorValue;
                        }

                        maxX = x;
                        maxY = y + separatorValue + 10;
                        x = 10;
                        y += separatorValue;
                    }
                    selected_container.UI_Grid_Planning_Squares = _uiGridSquares;

                    if (i == 0 && j == 0)
                    {
                        UI_squares_draw_Direction_Max_X = maxX;
                        UI_squares_draw_Direction_Max_Y = maxY;

                        UI_Draw_Grid_Support(gridsquarePanel, UI_squares_draw_Direction_Max_X, UI_squares_draw_Direction_Max_Y);

                        Point p1 = new Point(maxX + 5, 0);
                        UI_Draw_Grid_Support("East", gridsquarePanel, p1);
                        Point p2 = new Point(0, maxY + 5);
                        UI_Draw_Grid_Support("South", gridsquarePanel, p2);

                    }
                    x = 10;
                    y = 10;
                    maxX = 1;
                    maxY = 1;
                }
            }
        }

        /// <summary>
        /// Action taken when a Grid_Square is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel_grid_square_planning_MouseClick(Object sender, MouseEventArgs e)
        {

            Select_Rectangle_Square(sender, e, selected_container);
            

        }

        /// <summary>
        /// This method handles mouseclick functions for individual UI_Grid_squares
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="_container"></param>
        /// <returns>A <see cref="UI_Grid_Planning_Container"/> with the selected square in a 2D array of <see cref="UI_Grid_Planning_Square"/></returns>
        private UI_Grid_Planning_Container Select_Rectangle_Square(Object sender, MouseEventArgs e, UI_Grid_Planning_Container _container)
        {

            UI_Grid_Planning_Square[,] _gridsquares = _container.UI_Grid_Planning_Squares;

            for (int m = 0; m < Shared_Constants.GRID_SQUARE_SIZE; m++)
            {
                for (int n = 0; n < Shared_Constants.GRID_SQUARE_SIZE; n++)
                {
                    UI_Grid_Planning_Square square = _gridsquares[m, n];

                    if (square.rectangle.Contains(e.Location))
                    {

                        square = Set_Selected_UI_Grid_Planning_Square(square);


                        return _container;
                    }
                }

            }
            return null;
        }



        /// <summary>
        /// Loads grid_squares 2d array from container, preserving selected squares.
        /// </summary>
        /// <param name="selected_container"></param>
        private void Load_Grid_Squares(UI_Grid_Planning_Container selected_container)
        {
            UI_Grid_Planning_Square[,] squares = selected_container.UI_Grid_Planning_Squares;

            for (int m = 0; m < Shared_Constants.GRID_SQUARE_SIZE; m++)
            {
                for (int n = 0; n < Shared_Constants.GRID_SQUARE_SIZE; n++)
                {
                    if (squares[m, n].Selected)
                    {
                        gridsquarePanel.Fill_Rectangle(squares[m, n].zone, squares[m, n].rectangle);
                    }
                    else
                    {
                        gridsquarePanel.Fill_Rectangle(GridSquare_Zoning.Initialized, squares[m, n].rectangle);
                    }

                }
            }

            UI_Draw_Grid_Support(gridsquarePanel, UI_squares_draw_Direction_Max_X, UI_squares_draw_Direction_Max_Y);

            Point p1 = new Point(UI_squares_draw_Direction_Max_X + 5, 0);
            UI_Draw_Grid_Support("East", gridsquarePanel, p1);
            Point p2 = new Point(0, UI_squares_draw_Direction_Max_Y + 5);
            UI_Draw_Grid_Support("South", gridsquarePanel, p2);
 
        }














        /// <summary>
        /// Redraw Grid maps and clears out all of the UI Containers that have selected values.
        /// </summary>
        /// <param name="containerPanel"></param>
        /// <param name="squarePanel"></param>
        private void Redraw_Grid_Map(UI_GridPanel containerPanel, UI_GridPanel squarePanel)
        {

            Reset_Selected();
            squarePanel.gridPanel.Refresh();
            containerPanel.gridPanel.Refresh();
            
            //clear out all contents of the UI GRID CONTAINERS and Associated square containers.

            
        }

        /// <summary>
        /// This Method is used only during Redrawing when the size of the containers are changed.
        /// </summary>
        private void Reset_Selected()
        {
       
            for (int i = 0; i < perviousSizeOfGrid; i++)
            {
                for (int j = 0; j < perviousSizeOfGrid; j++)
                {

                    UI_Grid_Planning_Container _UI_Container = GridPlanner_Map[i, j];
                    _UI_Container.selected = false; //Resets the selected


                    UI_Grid_Planning_Square[,] _UI_Squares = _UI_Container.UI_Grid_Planning_Squares;
                    for (int k = 0; k < Shared_Constants.GRID_SQUARE_SIZE; k++)
                    {
                        for (int  m = 0; m < Shared_Constants.GRID_SQUARE_SIZE; m++)
                        {
                            _UI_Squares[k, m].Selected = false; //resets the selected
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Obtains the size of UI_Grid_Containers based on the Combox box selection
        /// </summary>
        /// <returns>Size of Grid</returns>
        private int GridSize()
        {
            
            string selected = comboBox_how_large.SelectedItem.ToString();
            int selectedSize = int.Parse(selected);
            int sqrtSelected = (int)Math.Sqrt(selectedSize);
            perviousSizeOfGrid = sqrtSelected;
            return sqrtSelected;
        }


        /// <summary>
        /// Handles selecting containers.
        /// </summary>
        /// <param name="selected_container"></param>
        /// <returns></returns>
        private UI_Grid_Planning_Container Set_Select_UI_Grid_Planning_Container(UI_Grid_Planning_Container selected_container)
        {
            //If true, reset the container.  if False, set to true and mark as selected
            if (previouslySelected == null)
            {
                previouslySelected = selected_container;
            }

            if (selected_container.selected)
            {
                selected_container.selected = false;
                gridcontainerPanel.Fill_Rectangle(GridSquare_Zoning.Initialized, selected_container.rect);
            }
            else
            {
                previouslySelected.selected = false;
                gridcontainerPanel.Fill_Rectangle(GridSquare_Zoning.Initialized, previouslySelected.rect);

                selected_container.selected = true;
                gridcontainerPanel.Fill_Rectangle(GridSquare_Zoning.Selected, selected_container.rect);
                previouslySelected = selected_container;
            }

            return selected_container;
        }


        /// <summary>
        /// Handles how to display Grid Panel squares when they are selected
        /// </summary>
        /// <param name="selected_square"></param>
        /// <returns>A selected <see cref="UI_Grid_Planning_Square"/></returns>
        private UI_Grid_Planning_Square Set_Selected_UI_Grid_Planning_Square(UI_Grid_Planning_Square selected_square)
        {
            if(selected_square.Selected)
            {
                selected_square.Selected = false;
                gridsquarePanel.Fill_Rectangle(GridSquare_Zoning.Initialized, selected_square.rectangle);
                selected_square.zone = GridSquare_Zoning.Initialized;
            }
            else
            {
                //Radio button determines what color to fill in the square
                selected_square.Selected = true;
                gridsquarePanel.Fill_Rectangle(selected_radioButton_gridzone, selected_square.rectangle);
                selected_square.zone = selected_radioButton_gridzone;
            }

            return selected_square;
        }

        /// <summary>
        /// Draws supporting grid outlines
        /// </summary>
        /// <param name="panel"></param>
        private void UI_Draw_Grid_Support(UI_GridPanel panel, int maxX, int maxY)
        {
            panel.gridPanelGraphics.DrawLine(panel.gridPanelPen, 1, 0, maxX, 0); //Draws horizontal graph line
            panel.gridPanelGraphics.DrawLine(panel.gridPanelPen, 1, 0, 1, maxY); //draws vertical graph line
        }

        /// <summary>
        /// Draws strings in support of grid outlines
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="panel"></param>
        private void UI_Draw_Grid_Support(string direction, UI_GridPanel panel, Point p1)
        {
            panel.gridPanelGraphics.DrawString(direction, panel.gridPanelFont, panel.gridPanelBrush, p1);
        }

        /// <summary>
        /// Determines which radio button is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllRadioButtons_CheckedChanged(Object sender, EventArgs e)
        {
            // Check of the raiser of the event is a checked Checkbox.
            // Of course we also need to to cast it first.
            if (((RadioButton)sender).Checked)
            {
                // This is the correct control.
                RadioButton rb = (RadioButton)sender;

                switch (rb.Name)
                {
                    case "radioButton_building":
                        selected_radioButton_gridzone = GridSquare_Zoning.Building;
                        break;
                    case "radioButton_road":
                        selected_radioButton_gridzone = GridSquare_Zoning.Infrustructure;
                        break;
                    case "radioButton_scenery":
                        selected_radioButton_gridzone = GridSquare_Zoning.Scenery;
                        break;
                    case "radioButton_water":
                        selected_radioButton_gridzone = GridSquare_Zoning.Water;
                        break;
                }
            }
        }

        /// <summary>
        /// Calculates and then stores adjacency list of values based on container and gridsquare
        /// </summary>
        private void Initialized_UI_Grid_Adjacency()
        {
            /**when looping through the ui_grid_map, need to mark adjacency matrix with edges 
             * an adjacent square must contain a 4 data points container location and square location, suggest creating custom class to handle these data points.  Adjacency_Compass class is a good name
             */
        }

    }
}
