using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealtorAppForm
{
    public partial class Form1 : Form
    {
        DataBase dataBase = new DataBase();         // переменная базы данных
        bool IsFound = false;                       // переменная для понятия того, был ли найден результат поисков из бд
        public Form1()
        {
            InitializeComponent();
            Font = Properties.Settings.Default.UserFont;
            BackColor = Properties.Settings.Default.UserColor;
            ForeColor = Properties.Settings.Default.UserColorText;
            StartPosition = FormStartPosition.CenterScreen; // центрирование приложения
            if (AppartmentRB.Checked)
            {
                BuyRB.Visible = true;
                RentRB.Visible = true;
            }
            for (int i = 1; i <= 5; i++)
            {
                RoomBuyCB.Items.Add(i);
            }
            for (int i = 1; i <= 5; i++)
            {
                RoomRentCB.Items.Add(i);
            }
            RoomBuyCB.SelectedIndex = 0;
            RoomRentCB.SelectedIndex = 0;
            BuyPanel.Visible = false;
            TermComboBox.Items.Add("Посуточно");
            TermComboBox.Items.Add("На длительный срок");
            TermHouseRentCheckBox.Items.Add("Посуточно");
            TermHouseRentCheckBox.Items.Add("На длительный срок");
            TermComboBox.SelectedIndex = 0;
            TermHouseRentCheckBox.SelectedIndex = 0;
            TrackBarLabel.Text = DistanceOfCityBuyTrack.Value + "км";
            TrackBarRentLabel.Text = DistanceOfCityRentTrack.Value + "км";
        }

        private void Select_Apartmets_For_Sale()       // метод выборки квартир на продажу
        {
            string general_request =
                $"SELECT *, Terrain.[Name] as 'TerrainName', Finishing.[Name] as 'finishing', Type_Of_House.[Name] as 'type_of_house', Owners.Number_Phone as 'owner_phone', Owners.name as 'owner_name' " +
                $"FROM Apartments_For_Sale JOIN Terrain " +
                $"ON Apartments_For_Sale.TerrainId = Terrain.Id " +
                $"JOIN Finishing " +
                $"ON Apartments_For_Sale.FinishingId = Finishing.Id " +
                $"JOIN Type_Of_House " +
                $"ON Apartments_For_Sale.Type_of_HouseId = Type_of_House.id " +
                $"JOIN Owners " +
                $"ON Apartments_For_Sale.OwnerID = Owners.Id";
            dataBase.OpenConnection();  // открытие подключения к бд
            SqlCommand cmd = new SqlCommand(general_request, dataBase.GetConnection());
            SqlDataReader general_dr = cmd.ExecuteReader();
            richTextBox1.Text += "\t\t\t\tВывод квартир на продажу:\n" +
                "====================================================================================\n\n";                    // вступление

            while (general_dr.Read())
            {
                int iter = 0;       // счётчик совпадений фильтров и даныых из бд
                if ((int)general_dr["Rooms"] == Convert.ToInt32(RoomBuyCB.SelectedItem.ToString()))                   // если значение кол-ва комнат из бд равно значению указанным пользователем
                    iter++;

                // условие начальной цены и конечной
                if (!string.IsNullOrEmpty(BegPriceApartBuyTB.Text))
                {
                    if ((int)general_dr["Price"] >= Convert.ToInt32(BegPriceApartBuyTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndPriceApartBuyTB.Text))
                {
                    if ((int)general_dr["Price"] <= Convert.ToInt32(EndPriceApartBuyTB.Text))
                        iter++;
                }
                else iter++;

                // условия начальной площади и конечной площади
                if (!string.IsNullOrEmpty(BegSquareApartBuyTB.Text))
                {
                    if ((int)general_dr["Square"] >= Convert.ToInt32(BegSquareApartBuyTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndSquareApartBuyTB.Text))
                {
                    if ((int)general_dr["Square"] <= Convert.ToInt32(EndSquareApartBuyTB.Text))
                        iter++;
                }
                else iter++;

                // условие начального этажа квартиры и конечного
                if (!string.IsNullOrEmpty(BegFloorApartBuyTB.Text))
                {
                    if ((int)general_dr["Stage"] >= Convert.ToInt32(BegFloorApartBuyTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndFloorApartBuyTB.Text))
                {
                    if ((int)general_dr["Stage"] <= Convert.ToInt32(EndFloorApartBuyTB.Text))
                        iter++;
                }
                else iter++;


                // условие начального этажа дома в котором находится квартира и конечного
                if (!string.IsNullOrEmpty(BegFloorApartBuyHouseTB.Text))
                {
                    if ((int)general_dr["Number_Of_Storeys"] >= Convert.ToInt32(BegFloorApartBuyHouseTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndFloorApartBuyHouseTB.Text))
                {
                    if ((int)general_dr["Number_Of_Storeys"] <= Convert.ToInt32(EndFloorApartBuyHouseTB.Text))
                        iter++;
                }
                else iter++;

                // условие новостройки и вторички
                if (((bool)general_dr["Type_Of_Object"] && NewBuildingRB.Checked) || (!(bool)general_dr["Type_Of_Object"] && ResaleRB.Checked) || (!ResaleRB.Checked && !NewBuildingRB.Checked))
                    iter++;

                // условие изолированных кормнат и смежных 
                if ((IsolatedRB.Checked && (bool)general_dr["Type_Of_Room"]) || (AdjacentRB.Checked && !(bool)general_dr["Type_Of_Room"]) || (!IsolatedRB.Checked && !AdjacentRB.Checked))
                    iter++;

                string typehouse = general_dr["type_of_house"].ToString();
                // условие материала из которого построен дом
                if (
                        BrickBuyCheckBox.Checked && BrickBuyCheckBox.Text == typehouse ||
                        MonolithicBuyCheckBox.Checked && MonolithicBuyCheckBox.Text == typehouse ||
                        PanelBuyCheckBox.Checked && PanelBuyCheckBox.Text == typehouse ||
                        WoodenBuyCheckBox.Checked && WoodenBuyCheckBox.Text == typehouse ||
                        BlockyBuyCheckBox.Checked && BlockyBuyCheckBox.Text == typehouse ||
                        !BlockyBuyCheckBox.Checked && !WoodenBuyCheckBox.Checked && !PanelBuyCheckBox.Checked && !MonolithicBuyCheckBox.Checked && !BrickBuyCheckBox.Checked
                    )
                    iter++;

                if (iter >= 12)
                {
                    IsFound = true;
                    richTextBox1.Text += general_dr["Rooms"].ToString() + "-ая, ";                  // кол-во комнат
                    richTextBox1.Text += general_dr["Price"].ToString() + "р., ";                   // цена
                    richTextBox1.Text += general_dr["Square"].ToString() + "м², ";                  // площадь
                    richTextBox1.Text += general_dr["Stage"].ToString() + "/";                      // этаж
                    richTextBox1.Text += general_dr["Number_Of_Storeys"].ToString() + ", ";         // этажнотсь
                    richTextBox1.Text += general_dr["type_of_house"].ToString() + ",";              // Тип дома (кирпичный, монолитный, панельный, деревянный, блочный)
                    if (Convert.ToBoolean(general_dr["Type_Of_Room"].ToString()))                   // Тип комнат (смежные или изолированные (0 - смежные, 1 - изолированные))
                        richTextBox1.Text += "Изолированные комн." + ",";
                    else
                        richTextBox1.Text += "Смежные комн." + ",";

                    if (Convert.ToBoolean(general_dr["Type_Of_Object"].ToString()))                 // Тип объекта (вторичка или новостройка (0 - вторичка, 1 - новостройка))
                        richTextBox1.Text += "Новостройка" + ",";
                    else
                        richTextBox1.Text += "Вторичка" + ",";

                    richTextBox1.Text += general_dr["finishing"].ToString() + ", ";                 // отделка
                    richTextBox1.Text += "ул. " + general_dr["Street"].ToString() + ", ";           // улица
                    richTextBox1.Text += general_dr["TerrainName"].ToString() + ", ";               // местность
                    richTextBox1.Text += general_dr["Year_Of_Construction"].ToString() + "г., ";    // год постройки
                    richTextBox1.Text += "с/у ";
                    if (Convert.ToBoolean(general_dr["Bathroom"].ToString()))                       // Раздельный ли санузел (0 - нет, 1 - да)
                        richTextBox1.Text += "раздельный " + ",";
                    else
                        richTextBox1.Text += "совмещённый " + ",";
                    richTextBox1.Text += general_dr["owner_name"].ToString() + " ";                 // Имя хозяйна объявления
                    richTextBox1.Text += general_dr["owner_phone"].ToString() + "\n\n";             // Номер хозяйна объявления
                    richTextBox1.Text += "====================================================================================\n\n";                    // отступ
                }
            }
            if (!IsFound) // если результат не был найдет, то не выводить сообщение
                richTextBox1.Text += "\t\t\tПо вашему запросов объявлений не найдено =( \n\n";
            IsFound = false;
            general_dr.Close();
        }
        // метод выборки квартир на аренду
        private void Select_Apartmets_For_Rent()
        {
            string general_request =
                $"SELECT *, Terrain.[Name] as 'TerrainName',  Owners.Number_Phone as 'owner_phone', Owners.name as 'owner_name' " +
                $"FROM Apartments_For_Rent JOIN Terrain " +
                $"ON Apartments_For_Rent.TerrainId = Terrain.Id " +
                $"JOIN Owners " +
                $"ON Apartments_For_Rent.OwnerID = Owners.Id";
            dataBase.OpenConnection();  // открытие подключения к бд
            SqlCommand cmd = new SqlCommand(general_request, dataBase.GetConnection());
            SqlDataReader general_dr = cmd.ExecuteReader();
            richTextBox1.Text += "\t\t\t\tВывод квартир по аренде:\n\n" +
                "====================================================================================\n\n";                    // вступление

            while (general_dr.Read())
            {
                int iter = 0;   // счётчик совпадений фильтров и даныых из бд
                if ((int)general_dr["Rooms"] == Convert.ToInt32(RoomRentCB.SelectedItem.ToString()))                   // если значение из бд равно значению указанным пользователем
                    iter++;

                // условие начальной цены и конечной
                if (!string.IsNullOrEmpty(BegPriceApartRentTB.Text))
                {
                    if ((int)general_dr["Price"] >= Convert.ToInt32(BegPriceApartRentTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndPriceApartRentTB.Text))
                {
                    if ((int)general_dr["Price"] <= Convert.ToInt32(EndPriceApartRentTB.Text))
                        iter++;
                }
                else iter++;

                // условия начальной площади и конечной площади
                if (!string.IsNullOrEmpty(BegSquareApartRentTB.Text))
                {
                    if ((int)general_dr["Square"] >= Convert.ToInt32(BegSquareApartRentTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndSquareApartRentTB.Text))
                {
                    if ((int)general_dr["Square"] <= Convert.ToInt32(EndSquareApartRentTB.Text))
                        iter++;
                }
                else iter++;

                // условие начального этажа квартиры и конечного
                if (!string.IsNullOrEmpty(BegFloorApartRentTB.Text))
                {
                    if ((int)general_dr["Stage"] >= Convert.ToInt32(BegFloorApartRentTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndFloorApartRentTB.Text))
                {
                    if ((int)general_dr["Stage"] <= Convert.ToInt32(EndFloorApartRentTB.Text))
                        iter++;
                }
                else iter++;


                // условие начального этажа дома в котором находится квартира и конечного
                if (!string.IsNullOrEmpty(BegFloorApartRentHouseTB.Text))
                {
                    if ((int)general_dr["Number_Of_Storeys"] >= Convert.ToInt32(BegFloorApartRentHouseTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndFloorApartRentHouseTB.Text))
                {
                    if ((int)general_dr["Number_Of_Storeys"] <= Convert.ToInt32(EndFloorApartRentHouseTB.Text))
                        iter++;
                }
                else iter++;

                // условие срока сдачи (0 - посуточно, 1 - длительный срок)
                if (TermComboBox.SelectedIndex == 0)
                {
                    if ((bool)general_dr["Deadline"] == false)
                        iter++;
                }
                else iter++;
                if (TermComboBox.SelectedIndex == 1)
                {
                    if ((bool)general_dr["Deadline"] == true)
                        iter++;
                }
                else iter++;

                if (iter >= 11)
                {
                    IsFound = true;
                    richTextBox1.Text += general_dr["Rooms"].ToString() + "-ая, ";                  // кол-во комнат
                    richTextBox1.Text += general_dr["Price"].ToString() + "р., ";                   // цена
                    richTextBox1.Text += general_dr["Deadline"].ToString() + ", ";                  // цена
                    richTextBox1.Text += general_dr["Square"].ToString() + "м², ";                  // площадь
                    richTextBox1.Text += general_dr["Stage"].ToString() + "/";                      // этаж
                    richTextBox1.Text += general_dr["Number_Of_Storeys"].ToString() + ", ";         // этажнотсь
                    richTextBox1.Text += "ул. " + general_dr["Street"].ToString() + ", ";           // улица
                    richTextBox1.Text += general_dr["TerrainName"].ToString() + ", ";               // местность
                    richTextBox1.Text += general_dr["owner_name"].ToString() + " ";                 // Id хозяйна объявления
                    richTextBox1.Text += general_dr["owner_phone"].ToString() + "\n\n";             // Id хозяйна объявления
                    richTextBox1.Text += "====================================================================================\n\n";                    // отступ
                }
            }
            if (!IsFound) // если результат не был найдет, то не выводить сообщение
                richTextBox1.Text += "\t\t\tПо вашему запросов объявлений не найдено =( \n\n";
            IsFound = false;
            general_dr.Close();
        }

        // метод выборки домов на продажу
        private void Select_Houses_For_Sale()
        {
            string general_request =
                $"SELECT *, Wall_Material.Name as 'MaterialName', Terrain.[Name] as 'TerrainName', Owners.Number_Phone as 'owner_phone', Owners.name as 'owner_name'" +
                $"FROM Houses_For_Sale " +
                $"JOIN Wall_Material " +
                $"ON Houses_For_Sale.Wall_MaterialId = Wall_Material.Id " +
                $"JOIN Terrain " +
                $"ON Houses_For_Sale.TerrainId = Terrain.Id " +
                $"JOIN Owners " +
                $"ON Houses_For_Sale.OwnerID = Owners.Id";
            dataBase.OpenConnection();  // открытие подключения к бд
            SqlCommand cmd = new SqlCommand(general_request, dataBase.GetConnection());
            SqlDataReader general_dr = cmd.ExecuteReader();
            richTextBox1.Text += "\t\t\t\tВывод домов на продажу:\n" +
                "====================================================================================\n\n";                    // отступление

            while (general_dr.Read())
            {
                int iter = 0;   // счётчик совпадений фильтров и даныых из бд

                string typehouse = general_dr["Name"].ToString();
                // условие типа дома (дом, коттедж, дача, таунхаус)
                if (
                        HouseCheckBox.Checked && HouseCheckBox.Text == typehouse ||
                        CottageCheckBox.Checked && CottageCheckBox.Text == typehouse ||
                        CountryHouseCheckBox.Checked && CountryHouseCheckBox.Text == typehouse ||
                        TownHouseCheckBox.Checked && TownHouseCheckBox.Text == typehouse ||
                        !HouseCheckBox.Checked && !CottageCheckBox.Checked && !CountryHouseCheckBox.Checked && !TownHouseCheckBox.Checked
                    )
                    iter++;

                // условие начальной цены и конечной дома
                if (!string.IsNullOrEmpty(BegPriceHouseBuyTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Price"]) >= Convert.ToInt32(BegPriceHouseBuyTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndPriceHouseBuyTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Price"]) <= Convert.ToInt32(EndPriceHouseBuyTB.Text))
                        iter++;
                }
                else iter++;

                // условия начальной площади и конечной площади дома
                if (!string.IsNullOrEmpty(BegSquareHouseBuyTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Square"]) >= Convert.ToInt32(BegSquareHouseBuyTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndSquareHouseBuyTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Square"]) <= Convert.ToInt32(EndSquareHouseBuyTB.Text))
                        iter++;
                }
                else iter++;

                // условие площади участка дома
                if (!string.IsNullOrEmpty(SquarePlotHouseBegTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Land_Area"]) >= Convert.ToInt32(SquarePlotHouseBegTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(SquarePlotHouseEndTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Land_Area"]) <= Convert.ToInt32(SquarePlotHouseEndTB.Text))
                        iter++;
                }
                else iter++;

                // условие начального этажа и конечного дома
                if (!string.IsNullOrEmpty(FloorHouseBegTB.Text))
                {
                    if ((int)general_dr["Floor"] >= Convert.ToInt32(FloorHouseBegTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(FloorHouseEndTB.Text))
                {
                    if ((int)general_dr["Floor"] <= Convert.ToInt32(FloorHouseEndTB.Text))
                        iter++;
                }
                else iter++;


                // условие расстояния дома до города 
                if (!string.IsNullOrEmpty(general_dr["Distance_To_City_Center"].ToString()))
                {
                    if ((int)general_dr["Distance_To_City_Center"] <= (int)DistanceOfCityBuyTrack.Value)
                        iter++;
                }
                else iter++;
                string typematerial = general_dr["MaterialName"].ToString();
                // условие материала стен дома
                if (BrickHouseBuyCheckBox.Checked && BrickHouseBuyCheckBox.Text == typematerial ||
                        TimberHouseBuyCheckBox.Checked && TimberHouseBuyCheckBox.Text == typematerial ||
                        GasBlockHouseBuyCheckBox.Checked && GasBlockHouseBuyCheckBox.Text == typematerial ||
                        MetalHosueBuyCheckBox.Checked && MetalHosueBuyCheckBox.Text == typematerial ||
                        PenBlocksHouseBuyCheckBox.Checked && PenBlocksHouseBuyCheckBox.Text == typematerial ||
                        SandwichHouseBuyCheckBox.Checked && SandwichHouseBuyCheckBox.Text == typematerial ||
                        ReinforcedHouseBuyCheckBox.Checked && ReinforcedHouseBuyCheckBox.Text == typematerial ||
                        WoodenBuyCheckBox.Checked && WoodenBuyCheckBox.Text == typematerial ||
                        ExperimentalHouseBuyCheckBox.Checked && ExperimentalHouseBuyCheckBox.Text == typematerial ||
                        !BrickHouseBuyCheckBox.Checked && !TimberHouseBuyCheckBox.Checked && !GasBlockHouseBuyCheckBox.Checked && !MetalHosueBuyCheckBox.Checked
                        && !PenBlocksHouseBuyCheckBox.Checked && !SandwichHouseBuyCheckBox.Checked && !ReinforcedHouseBuyCheckBox.Checked
                        && !WoodenBuyCheckBox.Checked && !ExperimentalHouseBuyCheckBox.Checked
                    )
                    iter++;

                if (iter >= 11)
                {
                    IsFound = true;
                    richTextBox1.Text += general_dr["Name"].ToString() + ", ";                              // тип
                    richTextBox1.Text += general_dr["Price"].ToString() + "р., ";                           // цена
                    richTextBox1.Text += general_dr["Square"].ToString() + "м², ";                          // площадь дома
                    richTextBox1.Text += general_dr["Land_Area"].ToString() + " соток, ";                   // Площадь участка (сколько соток)
                    if (general_dr["Distance_To_City_Center"].ToString() != "")                             // расстояние от дома до города
                        richTextBox1.Text += general_dr["Distance_To_City_Center"].ToString() + "км. до города, стены: ";
                    richTextBox1.Text += general_dr["MaterialName"].ToString() + ", ";                      // Материал стен дома
                    richTextBox1.Text += general_dr["Floor"].ToString() + "эт., ";                          // этажей в доме
                    richTextBox1.Text += general_dr["Street"].ToString() + ", ";                            // улица дома
                    richTextBox1.Text += general_dr["TerrainName"].ToString() + ", ";                       // город нахождения дома
                    richTextBox1.Text += general_dr["owner_name"].ToString() + ", ";                        // имя хозяйна объявления
                    richTextBox1.Text += general_dr["owner_phone"].ToString() + "\n\n";                     // номер хозяйна объявления
                    richTextBox1.Text += "====================================================================================\n\n";                  // отступ
                }

            }
            if (!IsFound) // если результат не был найдет, то не выводить сообщение
                richTextBox1.Text += "\t\t\tПо вашему запросу объявлений не найдено =( \n\n";
            IsFound = false;
            general_dr.Close();
        }

        private void Select_Houses_For_Rent()       // метод выборки домов на сдачу
        {
            string general_request =
                $"SELECT *, Wall_Material.Name as 'MaterialName', Terrain.[Name] as 'TerrainName', Owners.Number_Phone as 'owner_phone', Owners.name as 'owner_name'" +
                $"FROM Houses_For_Rent " +
                $"JOIN Wall_Material " +
                $"ON Houses_For_Rent.Wall_MaterialId = Wall_Material.Id " +
                $"JOIN Terrain " +
                $"ON Houses_For_Rent.TerrainId = Terrain.Id " +
                $"JOIN Owners " +
                $"ON Houses_For_Rent.OwnerID = Owners.Id";
            dataBase.OpenConnection();  // открытие подключения к бд
            SqlCommand cmd = new SqlCommand(general_request, dataBase.GetConnection());
            SqlDataReader general_dr = cmd.ExecuteReader();
            richTextBox1.Text += "\t\t\t\tВывод домов на сдачу:\n" +
                "====================================================================================\n\n";                    // вступление
            while (general_dr.Read())
            {
                int iter = 0;   // счётчик совпадений фильтров и даныых из бд

                string typehouse = general_dr["Name"].ToString();
                // условие типа дома (дом, коттедж, дача, таунхаус)
                if (
                        HouseCheckBox.Checked && HouseCheckBox.Text == typehouse ||
                        CottageCheckBox.Checked && CottageCheckBox.Text == typehouse ||
                        CountryHouseCheckBox.Checked && CountryHouseCheckBox.Text == typehouse ||
                        TownHouseCheckBox.Checked && TownHouseCheckBox.Text == typehouse ||
                        !HouseCheckBox.Checked && !CottageCheckBox.Checked && !CountryHouseCheckBox.Checked && !TownHouseCheckBox.Checked
                    )
                    iter++;

                // условие начальной цены и конечной дома
                if (!string.IsNullOrEmpty(BegPriceHouseRentTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Price"]) >= Convert.ToInt32(BegPriceHouseRentTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndPriceHouseRentTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Price"]) <= Convert.ToInt32(EndPriceHouseRentTB.Text))
                        iter++;
                }
                else iter++;

                // условия начальной площади и конечной площади дома
                if (!string.IsNullOrEmpty(BegSquareHouseRentTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Square"]) >= Convert.ToInt32(BegSquareHouseRentTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(EndSquareHouseRentTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Square"]) <= Convert.ToInt32(EndSquareHouseRentTB.Text))
                        iter++;
                }
                else iter++;

                // условие площади участка дома
                if (!string.IsNullOrEmpty(SquarePlotHouseBegRentTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Land_Area"]) >= Convert.ToInt32(SquarePlotHouseBegRentTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(SquarePlotHouseEndRentTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Land_Area"]) <= Convert.ToInt32(SquarePlotHouseEndRentTB.Text))
                        iter++;
                }
                else iter++;

                // условие начального этажа и конечного дома
                if (!string.IsNullOrEmpty(FloorHouseBegRentTB.Text))
                {
                    if ((int)general_dr["Floor"] >= Convert.ToInt32(FloorHouseBegRentTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(FloorHouseEndRentTB.Text))
                {
                    if ((int)general_dr["Floor"] <= Convert.ToInt32(FloorHouseEndRentTB.Text))
                        iter++;
                }
                else iter++;

                // условие расстояния дома до города 
                if (!string.IsNullOrEmpty(general_dr["Distance_To_City_Center"].ToString()))
                {
                    if ((int)general_dr["Distance_To_City_Center"] <= (int)DistanceOfCityRentTrack.Value)
                        iter++;
                }
                else iter++;

                // условие срока сдачи (0 - посуточно, 1 - длительный срок)
                if (TermHouseRentCheckBox.SelectedIndex == 0)
                {
                    if ((bool)general_dr["Deadline"] == false)
                        iter++;
                }
                else iter++;
                if (TermHouseRentCheckBox.SelectedIndex == 1)
                {
                    if ((bool)general_dr["Deadline"] == true)
                        iter++;
                }
                else iter++;

                string typematerial = general_dr["MaterialName"].ToString();
                // условие материала стен дома
                if (BrickHouseRentCheckBox.Checked && BrickHouseRentCheckBox.Text == typematerial ||
                        TimberHouseRentCheckBox.Checked && TimberHouseRentCheckBox.Text == typematerial ||
                        GasBlockHouseRentCheckBox.Checked && GasBlockHouseRentCheckBox.Text == typematerial ||
                        MetalHouseRentCheckBox.Checked && MetalHouseRentCheckBox.Text == typematerial ||
                        PenBlocksHouseRentCheckBox.Checked && PenBlocksHouseRentCheckBox.Text == typematerial ||
                        SandwichHouseRentCheckBox.Checked && SandwichHouseRentCheckBox.Text == typematerial ||
                        ReinforcedHouseRentCheckBox.Checked && ReinforcedHouseRentCheckBox.Text == typematerial ||
                        WoodenRentCheckBox.Checked && WoodenRentCheckBox.Text == typematerial ||
                        ExperimentalHouseRentCheckBox.Checked && ExperimentalHouseRentCheckBox.Text == typematerial ||
                        !BrickHouseRentCheckBox.Checked && !TimberHouseRentCheckBox.Checked && !GasBlockHouseRentCheckBox.Checked && !MetalHouseRentCheckBox.Checked
                        && !PenBlocksHouseRentCheckBox.Checked && !SandwichHouseRentCheckBox.Checked && !ReinforcedHouseRentCheckBox.Checked
                        && !WoodenRentCheckBox.Checked && !ExperimentalHouseRentCheckBox.Checked
                    )
                    iter++;

                if (iter >= 13)
                {
                    IsFound = true;
                    richTextBox1.Text += general_dr["Name"].ToString() + ", ";                              // тип
                    richTextBox1.Text += general_dr["Price"].ToString() + "р., ";                           // цена
                    richTextBox1.Text += general_dr["Square"].ToString() + "м², ";                          // площадь дома
                    richTextBox1.Text += general_dr["Land_Area"].ToString() + " соток, ";                   // Площадь участка (сколько соток)
                    if (general_dr["Distance_To_City_Center"].ToString() != "")                             // расстояние от дома до города
                        richTextBox1.Text += general_dr["Distance_To_City_Center"].ToString() + "км. до города, стены: ";
                    richTextBox1.Text += general_dr["MaterialName"].ToString() + ", ";                      // Материал стен дома
                    if (!(bool)general_dr["Deadline"])
                        richTextBox1.Text += "посуточно, ";
                    else
                        richTextBox1.Text += "на длительный срок, ";
                    richTextBox1.Text += general_dr["Floor"].ToString() + "эт., ";                          // этажей в доме
                    richTextBox1.Text += general_dr["Street"].ToString() + ", ";                            // улица дома
                    richTextBox1.Text += general_dr["TerrainName"].ToString() + ", ";                       // город нахождения дома
                    richTextBox1.Text += general_dr["owner_name"].ToString() + ", ";                        // имя хозяйна объявления
                    richTextBox1.Text += general_dr["owner_phone"].ToString() + "\n\n";                     // номер хозяйна объявления
                    richTextBox1.Text += "====================================================================================\n\n";                  // отступ
                }
            }
            if (!IsFound) // если результат не был найдет, то не выводить сообщение
                richTextBox1.Text += "\t\t\tПо вашему запросу объявлений не найдено =( \n\n";
            IsFound = false;
            general_dr.Close();
        }

        private void Select_Land_Plots()    // метод для выборки земельных участков
        {
            string general_request =
                $"SELECT *,  Land_Сategory.Name as 'LandCategory', Terrain.[Name] as 'TerrainName', Owners.Number_Phone as 'owner_phone', Owners.name as 'owner_name'" +
                $"FROM Land_Plots " +
                $"JOIN Terrain " +
                $"ON Land_Plots.TerrainId = Terrain.Id " +
                $"JOIN Land_Сategory " +
                $"ON Land_Plots.Land_CategoryId = Land_Сategory.Id " +
                $"JOIN Owners " +
                $"ON Land_Plots.OwnerID = Owners.Id";
            dataBase.OpenConnection();  // открытие подключения к бд
            SqlCommand cmd = new SqlCommand(general_request, dataBase.GetConnection());
            SqlDataReader general_dr = cmd.ExecuteReader();
            richTextBox1.Text += "\t\t\t\tВывод земельныъ участков:\n" +
                "====================================================================================\n\n";                    // вступление
            while (general_dr.Read())
            {
                int iter = 0;   // счетчик совпадений

                // условие начальной и конечной цены земельного участка
                if (!string.IsNullOrEmpty(PlotPriceBegTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Price"]) >= Convert.ToInt32(PlotPriceBegTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(PlotPriceEndTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Price"]) <= Convert.ToInt32(PlotPriceEndTB.Text))
                        iter++;
                }
                else iter++;

                // условие земельных категорий (ИЖС, СНТ, промназначение)
                if (SettlementsPlotCheckBox.Checked && general_dr["LandCategory"].ToString() == SettlementsPlotCheckBox.Text ||
                    AgriculturalPlotCheckBox.Checked && general_dr["LandCategory"].ToString() == AgriculturalPlotCheckBox.Text + " (СНТ, ДНП)" ||
                    IndustrialPlotCheckBox.Checked && general_dr["LandCategory"].ToString() == IndustrialPlotCheckBox.Text ||
                    !SettlementsPlotCheckBox.Checked && !AgriculturalPlotCheckBox.Checked && !IndustrialPlotCheckBox.Checked
                    )
                    iter++;

                // условие начальной и конечной площади земельного участка
                if (!string.IsNullOrEmpty(SquarePlotBegTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Square"]) >= Convert.ToInt32(SquarePlotBegTB.Text))
                        iter++;
                }
                else iter++;
                if (!string.IsNullOrEmpty(SquarePlotEndTB.Text))
                {
                    if (Convert.ToInt32(general_dr["Square"]) <= Convert.ToInt32(SquarePlotEndTB.Text))
                        iter++;
                }
                else iter++;

                if (general_dr["Distance_To_City_Center"].ToString() != "")
                {
                    if (Convert.ToInt32(general_dr["Distance_To_City_Center"]) <= DistanceOfCityPlotTrack.Value)
                        iter++;
                }
                else iter++;


                if (iter >= 6)
                {
                    IsFound = true;
                    if (Convert.ToBoolean(general_dr["Rent_Or_Buy"].ToString()))                            // Сдаётся или продаётся участок (0 - сдаётся, 1 - продаётся)
                        richTextBox1.Text += "продажа, ";
                    else
                        richTextBox1.Text += "сдача, ";
                    richTextBox1.Text += general_dr["Price"].ToString() + "р., ";                           // цена
                    richTextBox1.Text += general_dr["Square"].ToString() + "м², ";                          // Площадь дома 
                    if (general_dr["Distance_To_City_Center"].ToString() != "")                             // расстояние до города
                        richTextBox1.Text += general_dr["Distance_To_City_Center"].ToString() + "км. до города, ";
                    richTextBox1.Text += general_dr["Street"].ToString() + ", ";                            // улица участка
                    richTextBox1.Text += general_dr["TerrainName"].ToString() + " ";                        // город нахождения участка
                    richTextBox1.Text += general_dr["LandCategory"].ToString() + " ";                       // категория участка                  
                    richTextBox1.Text += general_dr["owner_name"].ToString() + ", ";                        // имя хозяйна объявления
                    richTextBox1.Text += general_dr["owner_phone"].ToString() + "\n\n";                       // номер хозяйна объявления
                    richTextBox1.Text += "====================================================================================\n\n";                  // отступ
                }
            }
            if (!IsFound) // если результат не был найдет, то не выводить сообщение
                richTextBox1.Text += "\t\t\tПо вашему запросу объявлений не найдено =( \n\n";
            IsFound = false;
            general_dr.Close();
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)   // кнопка сохранения 
        {
            if (richTextBox1.Text == "")
            {
                MessageBox.Show("нечего сохранять!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                File.WriteAllText(saveFileDialog1.FileName, richTextBox1.Text);
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)     // кнопка открытия сохранённых документов
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите файл...";
            openFileDialog.Filter = "DOC document|*.doc|DOCX document|*.docx|TXT Files|*.txt";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string temp = File.ReadAllText(openFileDialog.FileName);
                if (!string.IsNullOrEmpty(temp))
                    richTextBox1.Text = temp;
                else MessageBox.Show("Выбранный файл невозможно считать!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void печатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDocument printDocument = new PrintDocument();

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;
            if (printDialog.ShowDialog() == DialogResult.OK)
                printDialog.Document.Print();
        }

        private void BuyRB_CheckedChanged(object sender, EventArgs e)
        {
            if (BuyRB.Checked)
            {
                RentRB.Checked = false;
                if (AppartmentRB.Checked)
                {
                    BuyPanel.Visible = true;
                    RentPanel.Visible = false;
                    BuyHousePanel.Visible = false;
                    RentHousePanel.Visible = false;
                    PlotPanel.Visible = false;
                }
                if (HousesRB.Checked)
                {
                    BuyHousePanel.Visible = true;
                    RentHousePanel.Visible = false;
                    BuyPanel.Visible = false;
                    RentPanel.Visible = false;
                    PlotPanel.Visible = false;
                }
            }

        }

        private void RentRB_CheckedChanged(object sender, EventArgs e)
        {
            if (RentRB.Checked)
            {
                BuyRB.Checked = false;
                if (AppartmentRB.Checked)
                {
                    RentPanel.Visible = true;
                    BuyPanel.Visible = false;
                    BuyHousePanel.Visible = false;
                    RentHousePanel.Visible = false;
                    PlotPanel.Visible = false;
                }
                if (HousesRB.Checked)
                {
                    RentHousePanel.Visible = true;
                    BuyHousePanel.Visible = false;
                    BuyPanel.Visible = false;
                    RentPanel.Visible = false;
                    PlotPanel.Visible = false;
                }
            }

        }

        private void LandRB_CheckedChanged(object sender, EventArgs e)
        {
            if (LandRB.Checked)
            {
                PlotPanel.Visible = true;
                BuyRentPanel.Visible = false;
                BuyHousePanel.Visible = false;
                BuyPanel.Visible = false;
                RentPanel.Visible = false;
                RentHousePanel.Visible = false;
            }
        }

        private void AppartmentRB_CheckedChanged(object sender, EventArgs e)
        {
            if (AppartmentRB.Checked)
                BuyRentPanel.Visible = true;
            if (BuyRB.Checked)
            {
                BuyPanel.Visible = true;
                RentPanel.Visible = false;
                BuyHousePanel.Visible = false;
                RentHousePanel.Visible = false;
                PlotPanel.Visible = false;

            }
            else if (RentRB.Checked)
            {
                RentPanel.Visible = true;
                PlotPanel.Visible = false;
                BuyHousePanel.Visible = false;
                BuyPanel.Visible = false;
                RentHousePanel.Visible = false;
            }
            else
            {
                BuyHousePanel.Visible = false;
                BuyPanel.Visible = false;
                RentHousePanel.Visible = false;
                RentPanel.Visible = false;
                PlotPanel.Visible = false;
            }
        }

        private void HousesRB_CheckedChanged(object sender, EventArgs e)
        {
            if (HousesRB.Checked)
            {
                BuyRentPanel.Visible = true;
                if (BuyRB.Checked)
                {
                    BuyHousePanel.Visible = true;
                    BuyPanel.Visible = false;
                    RentPanel.Visible = false;
                    RentHousePanel.Visible = false;
                    PlotPanel.Visible = false;
                }
                else if (RentRB.Checked)
                {
                    RentHousePanel.Visible = true;
                    BuyHousePanel.Visible = false;
                    BuyPanel.Visible = false;
                    RentPanel.Visible = false;
                    PlotPanel.Visible = false;
                }
                else
                {
                    BuyHousePanel.Visible = false;
                    BuyPanel.Visible = false;
                    RentPanel.Visible = false;
                    RentHousePanel.Visible = false;
                    PlotPanel.Visible = false;
                }

            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (DistanceOfCityBuyTrack.Value > 100)
                TrackBarLabel.Text = "100+ км.";
            else
                TrackBarLabel.Text = DistanceOfCityBuyTrack.Value + " км.";
        }

        private void DistanceOfCityRentTrack_Scroll(object sender, EventArgs e)
        {
            if (DistanceOfCityRentTrack.Value > 100)
                TrackBarRentLabel.Text = "100+ км.";
            else
                TrackBarRentLabel.Text = DistanceOfCityRentTrack.Value + " км.";
        }

        private void DistanceOfCityPlotTrack_Scroll(object sender, EventArgs e)
        {
            if (DistanceOfCityPlotTrack.Value > 100)
                DistanceOfCityPlotTrackLabel.Text = "100+ км.";
            else
                DistanceOfCityPlotTrackLabel.Text = DistanceOfCityPlotTrack.Value + " км.";

        }

        private void SearchButton_Click(object sender, EventArgs e)     // кнопка запуска поиска по базе данных 
        {
            if (AppartmentRB.Checked)   // если были выбраны квартиры на выборку
            {
                if (BuyRB.Checked)       // если у квартир выбран пункт "купить"
                    Select_Apartmets_For_Sale();

                if (RentRB.Checked)     // если у квартир выбран пункт "снять"
                    Select_Apartmets_For_Rent();
            }
            if (HousesRB.Checked)   // если были выбраны дома на выборку
            {
                if (BuyRB.Checked)       // если у домов выбран пункт "купить"
                    Select_Houses_For_Sale();

                if (RentRB.Checked)     // если у домов выбран пункт "снять"
                    Select_Houses_For_Rent();
            }
            if (LandRB.Checked)     // если были выбраны земельные участки
                Select_Land_Plots();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void поддержкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("mail: suzmac@mail.ru | WhatsApp: 89184116069", "Контактные данные");
        }


        // метод шрифта по умолчанию
        private void поУмолчаниюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Font = DefaultFont;
            Properties.Settings.Default.UserFont = DefaultFont;                     // запись шрифта текста в настройки проекта
            Properties.Settings.Default.Save();                                     // сохранение настроек проекта 
        }

        // метод изменения шрифта в форме
        private void изменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog myFontDialog = new FontDialog();
            if (myFontDialog.ShowDialog() == DialogResult.OK)
            {
                Font = myFontDialog.Font;
                Properties.Settings.Default.UserFont = myFontDialog.Font;           // запись шрифта текста в настройки проекта
                Properties.Settings.Default.Save();                                 // сохранение настроек проекта 
            }
        }

        // метод изменения цвета текста 
        private void изменитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ColorDialog myColorDialog = new ColorDialog();
            if (myColorDialog.ShowDialog() == DialogResult.OK)
            {
                this.ForeColor = myColorDialog.Color;
                Properties.Settings.Default.UserColorText = myColorDialog.Color;    // запись цвета текста в настройки проекта
                Properties.Settings.Default.Save();                                 // сохранение настроек проекта 
            }    
        }

        // метод цвета текста по умолчанию
        private void поУмлочаниюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ForeColor = Color.Black;
            Properties.Settings.Default.UserColorText = Color.Black;                // запись цвета текста в настройки проекта
            Properties.Settings.Default.Save();                                     // сохранение настроек проекта 
        }

        // метод изменения цвета формы
        private void изменитьToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ColorDialog myColorDialog = new ColorDialog();
            if (myColorDialog.ShowDialog() == DialogResult.OK)
            {
                this.BackColor = myColorDialog.Color;
                Properties.Settings.Default.UserColor = myColorDialog.Color;        // запись цвета формы в настройки проекта
                Properties.Settings.Default.Save();                                 // сохранение настроек проекта 
            }    
        }

        // метод цвета формы по умолчанию
        private void поУмолчаниюToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(255, 192, 128);
            Properties.Settings.Default.UserColor = Color.FromArgb(255, 192, 128);  // запись цвета формы в настройки проекта
            Properties.Settings.Default.Save();                                     // сохранение настроек проекта 
        }
    }
}