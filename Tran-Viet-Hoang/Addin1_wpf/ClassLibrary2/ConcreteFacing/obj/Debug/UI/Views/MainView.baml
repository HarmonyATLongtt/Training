   M S B A M L   `   `   `  ����  ConcreteFacing&     ConcreteFacing.UI.Views.MainView   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   >d2http://schemas.microsoft.com/expression/blend/2008    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          �Name$  window��5   	    � WindowStartupLocation$ CenterScreen=�5   	   $	��700��5	   8   $
��1500��6E    � 	Resources 5
       �� 
TargetType �� Property �� Value   CateTemplate d� 	IsChecked �� UpdateSourceTrigger �� Mode  CoverFaceTemplate	 � RowDefinitions��%)  '��       '�� �     &	  *    &	 -    �� 5   
   " ��6   .�5      ��  � ��$ 5,5,5,5q�6'   �� 5       � ��$ 80��6&   �� 5       � ��$ 25��6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 5q�6'   �� 5       � ��$
 Left=�6;   �� 5       � ��$ Center=�69   5      �� 5   
   .R�5      �� $
��auto��5      $
��auto��6)   $��Center=�5      $��Center=�5      $��1q�5      
���   � 5      �� *CateIsChecked+$ PropertyChanged=�$ TwoWay=���5      �� *
CateName+5      5       �� 5"   
   .R�5#      � 	 5$      �� 5%      $��* �6$   �� 5&      $��40 �6$   .+�5)      �� $��0��6   $��Uniform=�5+      ��5*      �� *CoverFaceImgSource+��5,      �� *	imgheight+��5-      �� *
imgwidth+65   �� 5.      $��1��6   $��Bottom=�51      $��Center=�66   ��5/      �� *CoverFaceContent+ 50      �� *CoverFaceIsCheck+$ PropertyChanged=�$ TwoWay=�51   U   52      53      55      .��56      � 	 57   
   �� 58      $��40 �6   �� 59      $��40 �6   �� 5:      $��30 �6   �� 5;      $��* �6   �� 5<      $��40 �6   
 � ColumnDefinitions
 5>   
   �� 5?      $��* �6   �� 5@      $��* �6   �� 5A      $
��1100 �6   �� 5B      $	��180 �6   .+�5E   
   �� $��0��6   $��Element Covers Applying��6"   $
��Left=�5F      $��2��63   $
��Bold"�5G      $��Italic$�6#   $��15(�66   �� 5I   
   $��1��6   $��0��6   $��Element Category List��6-   �� 5J   
   $��1��6   $��2��6   $��Cover Setting Data��6-   �� 5L   
   $��0��6   $��2��6#   $��2��60   $��2��6D   $��5q�6U   #��[  5N      ��5M      �� *SourceCatelv+ �� SelectedItem 5O      �� *SelectedCate+$ PropertyChanged=�$ TwoWay=�5P      �� 5R   
   $��2��6   $��2��6"   $��LeftToRight=�62    �� Orientation$ 
Horizontal=�6N   .+�5S      �� $��Thickness (mm): ��6   $	��100��6/   $
��auto��6;   $��0q�6I   �� 5T      $
��auto��6   $��Center=�6&   $��5q�6C   $��30��6N   ��5U      �� *SelectedCate.Thickness+$ PropertyChanged=�$ TwoWay=�5V      5W      �� 5Y   
   -   5        $��lstVElemFace��5Z      $��3��5Y      $��2��6"   $��2��62   $��5q�6G   #��[ 5\      ��5[      �� **(SelectedCate.TemplateCoverFaceViewModels+$ PropertyChanged=�$ TwoWay=���5]      �� 5^      .R�5_      A�  A� Rows$ 2��6"    A� Columns$ 3��6+   69   5`      5b      �� 5d   
   $��4��6   $��3��6"   $��4��62   $ 
Horizontal=�5e      $��RightToLeft=�6.   .+�5f      �� $��Cancel��6   ��6(   �� *CloseCommand+��5g      ��  �� ElementName$ window��$ OneWay=��� 5h      $��OK��6    �� 	IsDefault . ��63   �� *CreateCommand+��5i      �� $ window��$ OneWay=�5j      5k      5l      