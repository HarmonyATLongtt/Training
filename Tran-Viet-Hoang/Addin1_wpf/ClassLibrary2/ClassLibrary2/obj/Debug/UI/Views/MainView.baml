   M S B A M L   `   `   `  ����  ClassLibrary2%    ClassLibrary2.UI.Views.MainView   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   >d2http://schemas.microsoft.com/expression/blend/2008    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          �Name$  window��5   	    � WindowStartupLocation$ CenterScreen=�5   	   $	��508��5	   	   $
��1300��6    � 	Resources 5
       �� 
TargetType �� Property �� Value��%>  '��       '�� �     �� 5   
   " ��6   .�5      ��  � ��$ 5,5,5,5q�6'   �� 5       � ��$ 80��6&   �� 5       � ��$ 25��6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 5q�6'   �� 5       � ��$
 Left=�6;   �� 5       � ��$ Center=�69   5      5      .��5      �  � RowDefinitions 5   
   �� 5      $��40 �6   �� 5      $��40 �6   �� 5      $��* �6   �� 5      $��40 �6    � ColumnDefinitions 5   
   �� 5       $	��150 �6   �� 5!      $	��150 �6   �� 5"      $��* �6   �� 5#      $	��180 �6   .+�5&   
   �� $��0��6   $��Etabs output file��6"   $
��Left=�6>   �� 5'   
   $��1��6   $��2��6"    �� 
IsReadOnly . $��5q�5(   $   $��25��6/   ��5)      �� *
FilePath+	 �� UpdateSourceTrigger$	 PropertyChanged=��� 5*   
   $��3��6   $
��Load��6!   $	��170��60   ��6=   �� *LoadCommand+�� 5,   
   $��1��6   $��0��6   $��Table sheets��6-   �� 5-   
   $��1��6   $��2��6   $��
Sheet data��6-   �� 5/   
   -   5        
 ��Name$
 
TablesList��50      $��0��5/      $��2��6"   $��2��6/   $��5q�6C    �� DisplayMemberPath$ 	TableName��53      ��51      �� *Tables+ �� SelectedItem 52      �� *TableSelected+ �� Mode$ TwoWay=�$	 PropertyChanged=�53      &   System.Windows.Controls.DataGrid  54   
   $��2��6   $��2��6#   $��2��60   $��5q�6E   $   HorizontalScrollBarVisibility$ Visible=�55      ��56      �� *TableSelected+$	 PropertyChanged=��� 58   
   $��3��6   $��3��6"   $��4��62    �� Orientation$ 
Horizontal=�59      $��RightToLeft=�6.   .+�5:      �� $��Cancel��6   ��6(   �� *CloseCommand+��5;      ��  �� ElementName$ window��$ OneWay=��� 5<      $��Create��6    �� 	IsDefault . ��67   �� *CreateCommand+��5=      �� $ window��$ OneWay=�5>      5?      5@      