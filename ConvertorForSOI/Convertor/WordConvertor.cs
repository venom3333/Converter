using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using Word = Microsoft.Office.Interop.Word;
using System.Drawing;
using System.Data;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

namespace ConvertorForSOI
{
	public class WordConvertor
	{
		/// <summary>
		/// Открываем поданный на вход doc файл, берем из него таблицу (если нет таблицы возвращаем ошибку).
		/// Возвращаем эту таблицу в виде DataTable и ещё одну таблицу с информацией из шапки.
		/// </summary>
		/// <param name="sourceFile">Месторасположение входящего word(doc) файла.</param>
		/// <returns>Набор таблиц в виде DataSet. Если произошла ошибка, то возвращаем Null.</returns>
		public static DataSet WordToTable(string sourceFile, Map map)
		{
			if (!File.Exists(sourceFile) || Path.GetExtension(sourceFile).ToLower() != ".doc")
			{
				MessageBox.Show("Укажите корректный источник данных.");
				return null;
			}

			DataSet ds = new DataSet();
			DataTable dtBody = new DataTable();
			DataTable dtHeader = new DataTable();

			Word.Application word = null;
			Word.Document docs = null;
			try
			{
				word = new Word.Application();
				docs = new Word.Document();

				object miss = System.Reflection.Missing.Value;
				object path = sourceFile;
				object readOnly = true;
				docs = word.Documents.Open(ref path, ref miss, false,
											   ref miss, ref miss, ref miss, ref miss,
											   ref miss, ref miss, ref miss, ref miss,
											   ref miss, ref miss, ref miss, ref miss,
											   ref miss);

				ds.DataSetName = Path.GetFileNameWithoutExtension(sourceFile).Replace(" ", "").Trim();
				dtBody.TableName = "Body";
				dtHeader.TableName = "Header";

				// Считываем текст который идёт перед таблицей.
				object start = 0;
				object end = docs.Tables[1].Cell(1, 1).Range.Start;
				Word.Range rng = docs.Range(ref start, ref end);
				rng.Select();
				string strHeader = rng.Text;

				// Парсим шапку doc файла и определяем какой формы документ.
				MapSourceItem.TypeDocument typeDocument = MapSourceItem.ParceHeader(strHeader);

				// Определяем номер с какого ряда будем заполнять таблицу.
				int rowNumber = map.GetRowNumberByTypeDocument(typeDocument);

				// Если в файле нет таблицы или в таблице нет рядов, то возвращаем ошибку.
				if (docs.Tables == null || docs.Tables.Count < 1 || docs.Tables[1].Rows == null || docs.Tables[1].Rows.Count < rowNumber)
					throw new ArgumentException("Некорректный word файл.");



				if (typeDocument == MapSourceItem.TypeDocument.NoType)
					throw new ArgumentException("Некорректный word файл.");

				// Заполняем таблицу dtHeader информацией из шапки. Первый ряд - название файла, второй ряд - текст шапки и третий ряд - тип документа.
				dtHeader.Columns.Add("Header");
				dtHeader.Rows.Add(sourceFile);
				dtHeader.Rows.Add(strHeader);
				dtHeader.Rows.Add(typeDocument.ToString());
				ds.Tables.Add(dtHeader);

				// В качестве названий колонок берем просто номера столбцов. 
				// Перечисление столбцов делаем в первом ряду в котором начинаются данные.
				// Номер этого ряда берём из msi.
				// Пока не встретим строку таблицы которая состоит хотябы минимум из двух ячеек 
				while (rowNumber < docs.Tables[1].Rows.Count)
				{
					if (docs.Tables[1].Rows[rowNumber].Cells.Count > 1)
					{
						for (int i = 0; i < docs.Tables[1].Rows[rowNumber].Cells.Count; i++)
						{
							dtBody.Columns.Add(i.ToString());
						}
						break;
					}
					rowNumber++;
				}

				for (int tableIdx = 1; tableIdx <= docs.Tables.Count; tableIdx++)
				{
					// переменная для привязки имен фотографий
					string fio = String.Empty;
					string fullFileName = String.Empty;

					// Цикл по всем таблицам документа (на случай если их больше одной)
					// Считывание данных из word таблицы начинаем с того ряда, который указан в msi. 
					for (int i = rowNumber; i <= docs.Tables[tableIdx].Rows.Count; i++)
					{
						DataRow dataRow = dtBody.NewRow();

						// флаг для привязки имен фотографий
						bool flagFIO = false;

						// Если левые строки в таблице, то пропускаем
						if (docs.Tables[tableIdx].Rows[i].Cells.Count < 2) { continue; }

						foreach (Word.Cell cell in docs.Tables[tableIdx].Rows[i].Cells)
						{
							// для названия фотографий
							if (flagFIO)
							{
								fio = ParseHelper.ClearString(cell.Range.Text).Replace(" ", "_").Trim();
							}

							// Если в ячейке изображение
							if (cell.Range.InlineShapes.Count > 0)
							{
								int shapeCounter = 0;

								// имя для директории с фотками
								string folderName = Path.GetFileNameWithoutExtension(sourceFile);
								foreach (Word.InlineShape shape in cell.Range.InlineShapes)
								{
									// выбрали текущий шейп
									shape.Select();
									// скопировали в клипборд как изображение
									word.Selection.CopyAsPicture();
									// проверка есть ли изображение в клипборде
									if (Clipboard.ContainsImage())
									{
										// соберем полное имя файла
										fullFileName = fio + "_" + shapeCounter + ".jpg";
										Path.GetInvalidFileNameChars().Aggregate(fullFileName, (current, c) => current.Replace(c.ToString(), string.Empty));
										// получаем изображение в переменную
										BitmapSource img = Clipboard.GetImage();
										// Текущий путь и имя файла
										string basePath = Directory.GetCurrentDirectory();
										// Создаем директорию под фотки, если еще не существует
										Directory.CreateDirectory(System.IO.Path.Combine(basePath, folderName));
										string fullPath = System.IO.Path.Combine(basePath, folderName, fullFileName);
										// Класс Encoder для jpeg
										JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
										// создать/перезаписать файл
										FileStream stream = File.Create(fullPath);
										// настройки Encoder'а
										jpegEncoder.QualityLevel = 80;
										// создаем фрейм изображения в энкодере
										jpegEncoder.Frames.Add(BitmapFrame.Create(img));
										// записываем в файл
										jpegEncoder.Save(stream);
										// Записываем название сохраненного файла
										dataRow[cell.ColumnIndex - 1] += fullFileName + ";";
										stream.Close();
										shapeCounter++;
									}
								}
								// Обнуляем строку с названием файла
								fio = String.Empty;
								fullFileName = String.Empty;
							}
							else
							{
								if (cell.Range.Text.ToLower().Contains("установочн"))
								{
									flagFIO = true;
								}
								// Избавимся от всех спец символов
								string cleanString = ParseHelper.ClearString(cell.Range.Text);
								// И запишем ее в наш DataRow
								if (String.IsNullOrWhiteSpace(cleanString))
								{
									dataRow[cell.ColumnIndex - 1] = String.Empty;
								}
								else
								{
									dataRow[cell.ColumnIndex - 1] = cleanString;
								}
							}

						}
						// Если ячейки пустые - не добавляем строку
						if (dataRow.ItemArray.Any(cell => !String.IsNullOrWhiteSpace(cell.ToString())))
						{
							dtBody.Rows.Add(dataRow);
						}

					}
				}
			}
			catch (Exception e)
			{
				docs.Close();
				word.Quit();
				MessageBox.Show(e.Message);
				return null;
			}
			finally
			{
				if (docs != null) docs.Close();
				if (word != null) word.Quit();
			}

			ds.Tables.Add(dtBody);
			return ds;
		}
	}
}
