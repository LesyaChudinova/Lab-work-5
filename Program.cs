using System.IO.Compression;
using System.Xml.Schema;
using System.Text.RegularExpressions;

namespace ConsoleApp5
{
    internal class Program
    {
        #region функции для вывода пунктов консольного меню

        /// <summary>
        /// Выводит все пункты основного консольного меню для навигации по программе
        /// </summary>
        static void PrintMainMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Выберите пункт меню:");
            Console.WriteLine("1. Сформировать двумерный массив");
            Console.WriteLine("2. Сформировать рваный массив");
            Console.WriteLine("3. Распечатать массивы");
            Console.WriteLine("4. Добавить К столбцов в конец двумерного массива");
            Console.WriteLine("5. Удалить первую строку, в которой встречается заданное число К");
            Console.WriteLine("6. Ввести строку символов");
            Console.WriteLine("7. Перевернуть в строке каждое предложение, заканчивающееся знаком '!'");
            Console.WriteLine("0. Выйти из программы");
            Console.WriteLine();
        }

        /// <summary>
        /// Выводит все пункты консольного меню для выбора способа задания массива
        /// </summary>
        static void PrintArrayCreatingMenu()
        {
            Console.WriteLine("Выберите способ формирования массива:");
            Console.WriteLine("1. Элементы вводятся с клавиатуры");
            Console.WriteLine("2. Элементы формируются с помощью датчика случайных чисел");
            Console.WriteLine();
        }

        /// <summary>
        /// Выводит все пункты консольного меню для выбора способа заполнения добавленных столбцов
        /// </summary>
        static void PrintFillingMehod()
        {
            Console.WriteLine("Введите способ, которым вы хотите заполнить добавленные столбцы: ");
            Console.WriteLine("1. Ввести элементы с клавиатуры");
            Console.WriteLine("2. Элементы заполнятся при помощи датчика случайных чисел");
            Console.WriteLine();
        }
        #endregion

        #region функции для проверки введенных данных
        /// <summary>
        /// Считывает значение, введенное пользователем и преобразовывает в целое число
        /// </summary>
        /// <param name="message"> Сообщение, которое будет видеть пользователь</param>
        /// <returns>Целое число</returns>
        static int ReadIntNumber(string message)
        {
            int number; //будущее целе число
            bool isConvert; //переменная, показывающая результат преобразования в целочисленный формат
            do
            {
                Console.WriteLine(message); //сообщение, которое увидит пользователь
                isConvert = Int32.TryParse(Console.ReadLine(), out number);
                if (!isConvert) Console.WriteLine($"Ошибка ввода. Пожалуйста, введите значение, являющееся целым числом, которое входит в границы этого типа (от {Int32.MinValue} до {Int32.MaxValue} включительно.");
            } while (!isConvert); //повторный запрос, пока число нельзя будет преобразовать
            
            return number; //преобразованное число
        }

        /// <summary>
        /// Проверяет, входит ли число в нужный диапазон значений
        /// </summary>
        /// <param name="number">Число, которое надо проверить</param>
        /// <param name="left">Левая граница диапазона</param>
        /// <param name="right">Правая граница диапазона</param>
        /// <returns>True, если число входит в диапазон, и false, если не входит</returns>
        static bool CheckRange(int number, int left, int right) => (number >= left && number <= right); //сокращенная запись функции, лямбда-выражение (если чисо больше левой и меньше правой границы допустимого диапазона, то функция истинна)

        /// <summary>
        /// Проверяет введенное значение на корректность ввода и преобразовывает его в целочисленный тип
        /// </summary>
        /// <param name="message">Сообщение, которое увидит пользователь</param>
        /// <param name="left">Левая граница диапазона, которому должно принадлежать число</param>
        /// <param name="right">Правая граница диапазона, которому должно принадлежать число</param>
        /// <returns>Целое число</returns>
        static int ReadAndCheckNumber(string message, int left, int right)
        {
            int number; //будущее целое число
            do
            {
                number = ReadIntNumber(message);
                if (!CheckRange(number, left, right)) Console.WriteLine($"Ошибка ввода. Пожалуйста, введите число еще раз. Оно должно быть больше {left} и меньше {right}");
            } while(number < left || number > right); //пока число не окажется в нужном диапазоне, повторно запрашиваем его у пользователя
            
            return number; //целое число
        }

        static char[] sentencesSeparators = { '.', '!', '?' }; //набор знаков препинания, разделяющих предложения
        static char[] wordsSeparators = { ',', ':', ';' }; //набор знаков препинания, разделяющих смысловые части предложений
        /// <summary>
        /// Проверяет, является ли символ допустимым знаком разделителем предложений
        /// </summary>
        /// <param name="element">Символ, который нужно проверить</param>
        /// <returns>True, если является, и false, если нет</returns>
        static bool IsSentencesSeparator(char element)
        {
            for (int i = 0; i < sentencesSeparators.Length; i++) //проход по набору знаков препинания, разделяющих предложения
                if (element == sentencesSeparators[i]) return true; //если среди них есть рассматриваемый символ => true
            return false;
        }

        /// <summary>
        /// Проверяет, является ли символ допустимым знаком разделителем слов
        /// </summary>
        /// <param name="element">Символ, который нужно проверить</param>
        /// <returns>True, если является, и false, если нет</returns>
        static bool IsWordsSeparator(char element)
        {
            for (int i = 0; i < wordsSeparators.Length; i++) //набор знаков препинания, разделяющих смысловые части предложений
                if (element == wordsSeparators[i]) return true; //если среди них есть рассматриваемый символ => true
            return false;
        }

        /// <summary>
        /// Проверяет, нет ли в строке двух символов подряд
        /// </summary>
        /// <param name="text">Строка</param>
        /// <returns>True, если нет двух символов подряд, и false, если есть</returns>
        static bool CheckDoubleSeparators(string text)
        {
            for (int i = 0; i < text.Length - 1; i++) //проход по длине строки
            {
                if (IsSentencesSeparator(text[i]) && IsSentencesSeparator(text[i + 1])) return false; //случай, когда встречается два разделителя предложений подряд
                if (IsWordsSeparator(text[i]) && IsWordsSeparator(text[i + 1])) return false; //случай, когда встречается два разделителя смысловых частей предложения подряд
                if (IsSentencesSeparator(text[i]) && IsWordsSeparator(text[i + 1])) return false; 
                if (IsWordsSeparator(text[i]) && IsSentencesSeparator(text[i + 1])) return false; //случаи, когда встречается разделитель предложений и разделитель смысловых его частей подряд
            }
            return true;
        }

        /// <summary>
        /// Читает строку и проверяет её на правильность ввода
        /// </summary>
        /// <param name="message">Сообщение, которое увидит пользователь</param>
        /// <returns>Строка</returns>     
        static string ReadAndCheckString(string message)
        {
            Regex pattern = new Regex(@"^[a-zA-ZА-Яа-яёЁ0-9\.\!\?\ \,\;\:]+[\!|\.|?]{1}$");
            //Данный шаблон можно расшифровать так: проверка идет от начала до конца строки, она не может быть пустой; в ней могут содержаться только символы, пренадлежащие диапазонам (х-х) и отдельным символам (х\х), указанным в квадратных скобках; на конце строки обязательно должен быть знак окончания предложения
            string text; //строка
            Console.WriteLine(message); //сообщение пользователю
            do
            {
                text = Console.ReadLine();
                if (!pattern.IsMatch(text) || !CheckDoubleSeparators(text)) Console.WriteLine("Ошибка ввода. Строка может состоять только из слов, разделенных пробелами (пробелов может быть несколько) и знаками препинания (,;:). В строке может быть несколько предложений. Каждон предложение заканчивается ровно одним знаком. В строке не должно быть 2 идущих подряд знаков препинания. Пожалуйста, введите строку еще раз.");
            } while (!pattern.IsMatch(text) || !CheckDoubleSeparators(text)); //повторный ввод, пока строка не пройдет проверку
            return text; //строка
        }
        #endregion

        #region функции для работы с двумерным массивом

        /// <summary>
        /// Cоздает массив с помощью ввода с клавиатуры
        /// </summary>
        /// <param name="rows">Количество строк матрицы</param>
        /// <param name="columns">Количество столбцов матрицы</param>
        /// <returns>Двумерный массив</returns>
        static int[,] CreateArray(int rows, int columns)
        {
            int[,] matrix = new int[rows, columns]; //нулевая матрица нужного размера
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = ReadAndCheckNumber($"Введите {j+1} элемент {i+1} строки матрицы:", Int32.MinValue, Int32.MaxValue); //каждый 0 из матрицы заменяется на число, введенное пользователем
                }
            }
            return matrix; //заполненная матрица
        }

        static Random random = new Random();
        /// <summary>
        /// Создает двумерный массив с помощью датчика случайных чисел
        /// </summary>
        /// <param name="rows">Количество строк матрицы</param>
        /// <param name="columns">Количество стоблцов матрицы</param>
        /// <returns>Двумерный массив</returns>
        static int[,] CreateRandomArray(int rows, int columns)
        {
            int[,] matrix = new int[rows, columns]; //нулевая матрица нужного размера
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = random.Next(-100, 100); //каждый 0 из матрицы заменяется на случайное число от -100 до 100
                }
            }
            return matrix; //заполненная матрица
        }

        /// <summary>
        /// Проверяет, пустой ли массив
        /// </summary>
        /// <param name="matrix">Массив, который нужно проверить</param>
        /// <returns>True, если массив пустой, и false, если нет</returns>
        static bool IsArrayEmpty(int[,] matrix) => matrix == null || matrix.Length == 0; //лямбда-выражение: если массив нулевой или его длина нулевая, то логическая функция станет истиной

        /// <summary>
        /// Печатает в консоль двумерный массив
        /// </summary>
        /// <param name="matrix">Массив, который нужно проверить</param>
        static void PrintArray(int[,] matrix)
        {
            if (IsArrayEmpty(matrix)) Console.WriteLine("Двумерный массив пуст."); //случай, когда массив пуст
            else
            {
                Console.WriteLine("Сформированный двумерный массив: ");
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                        Console.Write($"{matrix[i, j], 6}"); //выбран формат, позволяющий красиво и ровно напечатать двумерный массив
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Добавляет в двумерный массив столбцы с элементами
        /// </summary>
        /// <param name="matrix">Двумерный массив, в который надо добавить столбцы</param>
        /// <param name="newColumns">Количество столбцов</param>
        /// <param name="method">Метод запонения столбцов</param>
        /// <returns>Двумерный массив с добавленными столбцами</returns>
        static int[,] AddColumns(int[,] matrix, int newColumns, int method)
        {
            int[,] newMatrix = new int[matrix.GetLength(0), matrix.GetLength(1) + newColumns];
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    newMatrix[i, j] = matrix[i, j];

            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = matrix.GetLength(1); j < newMatrix.GetLength(1); j++)
                {
                    if (method == 1) newMatrix[i, j] = ReadAndCheckNumber($"Введите {i+1} элемент {j+1} столбца: ", -100, 100);
                    else newMatrix[i, j] = random.Next (-100, 100);
                }
            return newMatrix;
        }

        #endregion

        #region функции для работы с рваным массивом
        /// <summary>
        /// Создает рваный массив с помощью ввода символов с клавиатуры
        /// </summary>
        /// <param name="strings">Количество строчек массива</param>
        /// <returns>Рваный массив</returns>
        static int[][] CreateArray(int strings)
        {
            int[][] array = new int[strings][];
            for (int i = 0; i < strings; i++)
            {
                int columns = ReadAndCheckNumber($"Введите количество символов в {i+1} строке массива: ", 1, 100);
                array[i] = new int[columns];
                for (int j = 0; j < columns; j++)
                {
                    array[i][j] = ReadAndCheckNumber($"Введите {j+1} элемент {i+1} строки: ", Int32.MinValue, Int32.MaxValue);
                }
            }
            return array;
        }

        /// <summary>
        /// Создает рваный массив с помощью датчика случайных чисел
        /// </summary>
        /// <param name="strings">Количество строк массива</param>
        /// <returns>Рваный массив</returns>
        static int[][] CreateRandomArray(int strings)
        {
            int[][] array = new int[strings][];
            for (int i = 0; i < strings; i++)
            {
                int columns = random.Next(1, 15);
                array[i] = new int[columns];
                for (int j = 0; j < columns; j++)
                {
                    array[i][j] = random.Next(-100,100);
                }
            }
            return array;
        }

        /// <summary>
        /// Проверяет, пустой ли массив
        /// </summary>
        /// <param name="array">Массив, который надо проверить</param>
        /// <returns>True, если массив пустой, и false, если нет</returns>
        static bool IsArrayEmpty(int[][] array) => array == null || array.Length == 0;

        /// <summary>
        /// Печатает массив в консоль
        /// </summary>
        /// <param name="array">Массив, который надо распечатать</param>
        static void PrintArray(int[][] array)
        {
            if (IsArrayEmpty(array)) Console.WriteLine("Рваный массив пуст.");
            else
            {
                Console.WriteLine("Сформированный рваный массив: ");
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array[i].GetLength(0); j++)
                    {
                        Console.Write($"{array[i][j], 6}");
                    }
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Удаляет строку, в которой встречается в первый раз элемент К
        /// </summary>
        /// <param name="array">Массив, который надо обработать</param>
        /// <param name="element">Искомый элемент</param>
        /// <returns>Рваный массив</returns>
        static int[][] DeleteRow(int[][] array, int element)
        {
            ///Локальная функция - находит первое вхождение искомого элемента в массив и возвращет номер строки, где он находится, или -1, если такого элемента нет
            int FindElement()
            {
                int elementRow = -1;
                for (int row = 0; row < array.GetLength(0); row++)
                {
                    foreach (int item in array[row])
                    {
                        if (item == element && elementRow == -1)
                        {
                            elementRow = row;
                            break;
                        }
                    }
                }
                return elementRow;
            }

            if (FindElement() == -1)
            {
                Console.WriteLine("Такого элемента в массиве нет.");
                return array;
            }
            else
            {
                int[][] newArray = new int[array.GetLength(0) - 1][];
                for (int i = 0; i < FindElement(); i++)
                    newArray[i] = array[i];
                for (int i = FindElement(); i < newArray.GetLength(0); i++)
                    newArray[i] = array[i + 1];
                Console.WriteLine("Строка удалена.");
                return newArray;
            }
            

        }
        #endregion

        #region функции для работы со строками
        /// <summary>
        /// Переворачивает предложения в строке, которые заканчиваются восклицательным знаком восклицания
        /// </summary>
        /// <param name="text">Строка</param>
        /// <returns>Строка с перевернутыми предложениями</returns>
        static string FlipSentence(string text)
        {
            bool isExclamation = false;
            foreach(char element in text)
            {
                if (IsSentencesSeparator(element)) text = text.Replace(Convert.ToString(element), Convert.ToString(element) + "*");
                if (element == '!') isExclamation = true;
            }
            if (isExclamation)
            {
                string[] splittedText = text.Split('*');
                for (int i = 0; i < splittedText.Length; i++)
                {
                    if (splittedText[i].EndsWith('!'))
                    {
                        string[] words = splittedText[i].TrimEnd('!').Trim(' ').Split(' ');
                        Array.Reverse(words);
                        splittedText[i] = String.Join(" ", words) + "!" + " ";
                    }
                }
                string newText = String.Join("", splittedText);
                return newText;
            }
            else return "В данной строке нет предложений, оканчивающихся восклицательным знаком.";
        }
        #endregion

        static void Main(string[] args)
        {
            int menuPoint;
            int matrixCreating;
            int ragArrayCreating;
            int[,] matrix = new int[0,0];
            int[][] ragArray = new int[0][];

            string text = "";
            do
            {
                PrintMainMenu();
                menuPoint = ReadAndCheckNumber("Выбранный вами пункт меню: ", 0, 8);
                Console.WriteLine();
                switch (menuPoint)
                {
                    case 1:
                        PrintArrayCreatingMenu();
                        matrixCreating = ReadAndCheckNumber("Выбранный вами способ задания массива: ", 1, 2);
                        Console.WriteLine();
                        switch (matrixCreating)
                        {
                            case 1:
                                matrix = CreateArray(ReadAndCheckNumber("Введите количество строк массива: ", 1, 100), ReadAndCheckNumber("Введите количество столбцов массива: ", 1, 100));
                                break;

                            case 2:
                                matrix = CreateRandomArray(ReadAndCheckNumber("Введите количество строк массива: ", 1, 100), ReadAndCheckNumber("Введите количество столбцов массива: ", 0, 100));
                                break;
                        }
                        Console.WriteLine("Двумерный массив сформирован.");
                        break;

                    case 2:
                        PrintArrayCreatingMenu();
                        ragArrayCreating = ReadAndCheckNumber("Выбранный вами способ задания массива: ", 1, 2);
                        Console.WriteLine();
                        switch (ragArrayCreating)
                        {
                            case 1:
                                ragArray = CreateArray(ReadAndCheckNumber("Введите количество строк массива: ", 0, 100));
                                break;

                            case 2:
                                ragArray = CreateRandomArray(ReadAndCheckNumber("Введите количество строк массива: ", 0, 100));
                                break;
                        }
                        Console.WriteLine("Рваный массив сформирован.");
                        break;

                    case 3:
                        PrintArray(matrix);
                        PrintArray(ragArray);
                        break;

                    case 4:
                        if (IsArrayEmpty(matrix)) Console.WriteLine("Двумерный массив пуст. Чтобы добавить столбцы, нужно сначала сформировать массив при помощи 1 пункта меню.");
                        else
                        {
                            int columnsNumber = ReadAndCheckNumber("Введите количество столбцов, которое вы хотите добавить: ", 0, 100);
                            Console.WriteLine();
                            PrintFillingMehod();
                            int fillingMethod = ReadAndCheckNumber("Выбранный вами метод заполнения добавленных столбцов: ", 1, 2);
                            matrix = AddColumns(matrix, columnsNumber, fillingMethod);
                            Console.WriteLine("Столбцы добавлены.");
                        }
                        break;

                    case 5:
                        if (IsArrayEmpty(ragArray)) Console.WriteLine("Рваный массив пуст. Чтобы удалить строку из массива, необходимо сначала сформировать его при помощи 2 пункта меню.");
                        else
                        {
                            int findItem = ReadAndCheckNumber("Введите искомый элемент К: ", Int32.MinValue, Int32.MaxValue);
                            Console.WriteLine();
                            ragArray = DeleteRow(ragArray, findItem);
                        }
                        break;

                    case 6:
                        {
                            text = ReadAndCheckString("Введите строку: ");
                            Console.WriteLine("Строка сформирована");
                            break;
                        }

                    case 7:
                        {
                            if (text.Length == 0) Console.WriteLine("Строка пуста.");
                            else
                            {
                                Console.WriteLine("Перевернутая строка: ");
                                Console.WriteLine(FlipSentence(text));
                            }
                            break;
                        }
                        

                }
            } while (menuPoint != 0);
        }
    }
}
