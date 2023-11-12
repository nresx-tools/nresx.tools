# ADR 002: ���� �������㳿 (��������) ��� ������ � ������� \*.xlsx ��� C\#

### ����: 10.11.2023

### ������: �������������

### ��������

���������� "�������" ��������� ������ � ������� \*.xlsx. �� ������ � ���� �� ������� ��� � ������ ����.
��� ������� �������� ��������, ��� ���� ������� �� ���������� ����� ������� ����� ��� ��������� ������������� MS Excel �� ������� �������.
���������� ��������� ������� ��'��, �� ������� ����, � ����� ���������� ��������� ��������� (�������� � ��������� ���� ������� �� ����) ��������� � ������ �������� ������ ��������.
����� ���������� ��������� ������� ��� ��������� �������� ���������� �������� �� ���� ��������� ����������� ��� �������������� ������������ � ��������� ���������.

### г�����

� �������� ������� ������ � ������� \*.xlsx ��� ������������ ����������� �������� �� ������� ���� � ���������� "�������", ���� �������� ������ ��������������� �������� EPPlus. 
�� �������� ������� ��� �������� ����� �����:
EPPlus �������� ������� �� ���������� ����� Excel ��� ��������� ������������� MS Excel �� ������� �������.
������ ����������� ������ �� ��������� ������� �������� ��'��� ����.
� ���� 5.� (������� �������� ����� - 7.�) ��������������� �� ������ [PolyForm Noncommercial License 1.0.0](https://polyformproject.org/licenses/noncommercial/1.0.0), 
��� �������� ��������������� ������� ����������� ��� ������������� ��������� �������.

### �������

1. �������� ��� ���������� ������������� ������������ ������� ����糿 (PolyForm Noncommercial 1.0.0) � ����� ������ ��� ��������� �� ����� �������� �����.
�������� ������������ �� ������ ������.
2. ����� ������� �� ������ ���� ����糿 � �������� (�� � 4.� ���� LGPL 3.0), 
�������� � ��� ���� ����糿 � ����������� � ����������� ��������� ���� ��������, �� ��������� ���� ���� ����������.

### ������������

���� ��������� ������� ������������:

| ���������                                                                           | ������ ��� MSExcel | �������� ������������  | ������� ��������  | ϳ������� ������   | �������� ������   | ��������� �����   | �������       |
|--------------------------------------------------------------------------------------|--------------------| ------------------------|--------------------|--------------------|--------------------|--------------------|----------------|
| [**Syncfusion**](https://www.syncfusion.com/document-processing/excel-framework/net) | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | **������ (2100ms)**| :heavy_check_mark: | :heavy_check_mark: **Comunity edition** |
| [**Spire.Xls**](https://www.e-iceblue.com/Introduce/excel-for-net-introduce.html)    | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | **������ (1800ms)**| :heavy_check_mark: | :heavy_check_mark: **Free edition with restrictions** |
| [EPPlus 7.x](https://www.epplussoftware.com)                                         | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | ������ (677ms)     | :x:                | :heavy_check_mark: [PolyForm Noncommercial 1.0.0](https://polyformproject.org/licenses/noncommercial/1.0.0)    |
| [NPOI](https://github.com/dotnetcore/NPOI)                                           | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | ������ (1700ms)    | :x:                | :heavy_check_mark: [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0)    |
| [Aspose.Cells](https://docs.aspose.com/cells/net)                                    | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | ������             | :heavy_check_mark: | :x: ���������� |
| [SmartXLS](https://www.smartxls.com/index.htm)                                       | :heavy_check_mark: | :heavy_multiplication_x:| :heavy_check_mark: | :heavy_check_mark: | ������             | :x:                | :x: Commercial opensource      |
| OpenXML                                                                              | :heavy_check_mark: | :heavy_multiplication_x:| :heavy_check_mark: | :heavy_check_mark: | ������             | :x:                | :heavy_check_mark: ����������� |
| Office interop                                                                       | :x:                | :x:                     | :heavy_check_mark: | :heavy_check_mark: | :x: ������         | :heavy_check_mark: | :heavy_check_mark: ����������� |


