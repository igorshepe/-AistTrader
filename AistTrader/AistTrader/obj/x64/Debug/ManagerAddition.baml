   M S B A M L   `   `   `  ����K  GStockSharp.Xaml, Version=4.3.15.0, Culture=neutral, PublicKeyToken=nullJ6clr-namespace:StockSharp.Xaml;assembly=StockSharp.XamlStockSharp.Xaml  5   	    
AistTrader    AistTrader.ManagerAddition   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5	      P LStockSharp.Licensing, Version=4.3.15.0, Culture=neutral, PublicKeyToken=null/xaml"http://schemas.stocksharp.com/xaml    5
      Asx6clr-namespace:StockSharp.Xaml;assembly=StockSharp.Xaml    5   	   -   5          �Name$  StratSettings��5   	    � Title$0 *Aist Trader - Add agent or group of agents��5   	    � 
ResizeMode$ NoResize=�5   	   $��359.2��5      $	��285��5   	    � Icon$# Resources/Images/mainIcon.ico��5   	    � WindowStartupLocation$ CenterScreen=�5   	   $��Consolas)�5   	   .��5      � $��1,0,1,12q�6    � 	Resources 5   
    �� 
TargetType �� Triggers P� Property	 P� Value
 �� Property �� Value �� RelativeSource �� Path    StockSharp.Xaml.UnitEditor1   +StockSharp.Xaml.PropertyGrid.SecurityEditora ]DevExpress.Xpf.Core.v15.2, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a3  -DevExpress.Xpf.Core.DXFrameworkContentElement��%v  '��       '  �     '  �    �� 5      " ��6    5      P� 5       �@=�HasError	 . . �5      �� 
 �@�ToolTip 64   ��  �� *Self+$) #(Validation.Errors)[0].ErrorContent�5      5      �� 5      "  6    5      P� 5       �@=�HasError	 . . �5      �� 
 �@�ToolTip 64   ��  �� *Self+$) #(Validation.Errors)[0].ErrorContent�5      5!      �� 5"      "  6    5#      P� 5$       �@=�HasError	 . . �5%      �� 
 �@ ToolTip 64   ��  �� *Self+$) #(Validation.Errors)[0].ErrorContent�5&      5(      5)      .+�5+   
   �� $��Select a portfolio��6   $��25��5,      $
��Left=�5-      $��1,0,0,0q�5.      $	��Top=�5/      $
��Left=�50      �� 51   
   $%��Select agent or group of agents��6   $��26��52      $
��Left=�53      $��1,53,0,0q�54      $	��Top=�55      $
��Left=�56      $	��228��57      �� 58   
   -   5        $��SecurityLabel��58      $��	Security:��59      $��25��5:      $
��Left=�5;      $��	1,160,0,0q�5<      $	��Top=�5=      $
��Left=�5>      �� 5?   
   -   5        $��	AmountLbl��5E      $��Amount��5?      $��25��5@      $
��Left=�5A      $��	1,217,0,0q�5B      $	��Top=�5C      $
��Left=�5D      5E      �� 5F   
   -   5         ��Name$ UnitVolumeLabel��5F      $
��Left=�5G      $��125,196,0,0q�5H      $	��Top=�5I      $	��116��5J      $��26��5K      �� 5L   
   -   5        $��PortfolioComboBox��5O      $��22��5L      $��5,25,5,0q�5N      $	��Top=�5P      $��0��5Q       �� Text 5M      �� $ 	Portfolio� �� ValidatesOnDataErrors .  �� UpdateSourceTrigger$ PropertyChanged=� �� SelectedItem 5T      �� $ SelectedPortfolio��� 5V   
   -   5         ��Name$  GroupOrSingleAgentComboBox��5Y      $��22��5V      $��5,79,5,0q�5X      $	��Top=�5[      $��1��5]       5W      �� $ GroupOrSingleAgent� . $ PropertyChanged=� 5\      �� $  SelectedGroupOrSingleAgent�5^      $   StockSharp.Xaml.SecurityEditor  5h   
   -   5        $��SecurityPickerSS��5i      $��	5,185,5,0q�5j      $��28��6    $	��Top=�6,   $��2��6D      SelectedSecurity 5k      �� $ Tools� . $ PropertyChanged=�5l        5o   
   -   5        $��AmountTextBox��5x      $
��Left=�5o      $��22��5p      $��	5,243,0,0q�5r       �� TextWrapping$
 Wrap=�5s      $	��Top=�5t      $	��115��5u      $��3��5v      #��[B 5y      ��5q      �� $ Amount� . $ PropertyChanged=�5y      �� 5{   
   -	   5        $��
OkBtnClick��5}      $
��ОК��5{      $��Right=�5|      $��0,0,85,8q�5~      $��75��5      $��4��5�       �� 	IsDefault . $��23��5�   "   $��Bottom=�6.   {�. 5�      �� 5�   
   $��Отмена��6   $��Right=�5�      $��0,0,5,8q�5�      $��75��5�      $��5��5�       �� IsCancel . $��23��5�      $��Bottom=�6   �� 5�   
   -
   5        $ AliasLbl��5�      $��Alias��6"   $
��Left=�62   $��	5,107,0,0q�6M   $	��Top=�6`   �� 5�   
   -   5         ��Name$ AliasTxtBox��5�      $��23��6'   $��	5,133,5,0q�63   $
 Wrap=�6F   $�� ��6Z   $	��Top=�6b   5�      5�      