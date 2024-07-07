using System;

namespace TokoGrosirApp
{
    public class Program
    {
        public static void Main()
        {
            Database database = new Database();
            while (true)
            {
                Console.WriteLine("1. Input Transaksi");
                Console.WriteLine("2. Tampilkan Transaksi");
                Console.WriteLine("3. Urutkan Transaksi");
                Console.WriteLine("4. Cari Barang berdasarkan Jenis");
                Console.WriteLine("5. Exit");
                Console.Write("Pilih opsi: ");
                string opsi = Console.ReadLine();

                switch (opsi)
                {
                    case "1":
                        TokoProgram.InputTransaction(database);
                        break;
                    case "2":
                        TokoProgram.DisplayTransactions(database);
                        break;
                    case "3":
                        TokoProgram.SortTransactions(database);
                        break;
                    case "4":
                        Console.Write("Masukkan jenis barang yang dicari: ");
                        string jenisBarang = Console.ReadLine();
                        var reader = database.ExecuteStoredProcedureWithParam("sp_SearchingByJenisBarang", "@jenisBarang", jenisBarang);

                        if (reader.HasRows)
                        {
                            Console.WriteLine($"Barang dengan jenis '{jenisBarang}' ditemukan:");
                            while (reader.Read())
                            {
                                Console.WriteLine($"{reader["merk_jenis"]} ({reader["ukuran"]})");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Tidak ada barang yang ditemukan dengan jenis tersebut.");
                        }
                        reader.Close(); // jangan lupa untuk menutup reader setelah digunakan
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Opsi tidak valid.");
                        break;
                }
            }
        }
    }
}
