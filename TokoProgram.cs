using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TokoGrosirApp
{
    public class TokoProgram
    {
        public class Produk
        {
            public string MerkJenis { get; set; }
        }

    public static void InputTransaction(Database database)
{
    try
    {
        Console.Write("Nama Pelanggan: ");
        string namaPelanggan = Console.ReadLine();

        List<Tuple<string, string, int>> productsToBuy = new List<Tuple<string, string, int>>();

        bool continueBuying = true;
        while (continueBuying)
        {
            var produkOptions = GetProductsFromDatabase(database);

            // Validasi jika tidak ada produk
            if (produkOptions.Count == 0)
            {
                Console.WriteLine("Tidak ada produk yang tersedia untuk dibeli.");
                return;
            }

            // Tampilkan pilihan produk
            Console.WriteLine("Pilih Merk/Jenis Produk:");
            int index = 1;
            foreach (var produk in produkOptions)
            {
                Console.WriteLine($"{index}. {produk.MerkJenis}");
                index++;
            }
            Console.Write("Pilih nomor: ");
            int produkPilihan = int.Parse(Console.ReadLine());
            var merkJenis = produkOptions[produkPilihan - 1].MerkJenis;

            // Pilihan Ukuran
            Console.WriteLine("Pilih Ukuran:");
            List<string> ukuranOptions = GetSizesFromDatabase(database, merkJenis);
            index = 1;
            foreach (var ukuranOption in ukuranOptions)
            {
                Console.WriteLine($"{index}. {ukuranOption}");
                index++;
            }
            Console.Write("Pilih nomor: ");
            int ukuranPilihan = int.Parse(Console.ReadLine());
            string ukuran = ukuranOptions[ukuranPilihan - 1];

            // Pilihan Jumlah
            Console.WriteLine("Pilih Jumlah (1-100):");
            int jumlah = int.Parse(Console.ReadLine());
            if (jumlah < 1 || jumlah > 100)
            {
                Console.WriteLine("Jumlah tidak valid. Harus antara 1 dan 100.");
                return;
            }

            productsToBuy.Add(new Tuple<string, string, int>(merkJenis, ukuran, jumlah));

            Console.Write("Beli produk lain? (Y/N): ");
            string buyAnother = Console.ReadLine();
            continueBuying = (buyAnother.ToUpper() == "Y");
        }

        // Processing each product to buy
        foreach (var product in productsToBuy)
        {
            string merkJenis = product.Item1;
            string ukuran = product.Item2;
            int jumlah = product.Item3;

            using (var connection = database.GetConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT stok, harga_jual FROM barang WHERE merk_jenis = @merkJenis AND ukuran = @ukuran";
                    command.Parameters.AddWithValue("@merkJenis", merkJenis);
                    command.Parameters.AddWithValue("@ukuran", ukuran);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int stock = reader.GetInt32(0);
                            decimal hargaJual = reader.GetDecimal(1);

                            if (stock >= jumlah)
                            {
                                reader.Close();

                                command.CommandText = "UPDATE barang SET stok = stok - @jumlah WHERE merk_jenis = @merkJenis AND ukuran = @ukuran";
                                command.Parameters.AddWithValue("@jumlah", jumlah);
                                command.ExecuteNonQuery();

                                decimal pajak = 0.1m * hargaJual * jumlah;
                                decimal totalHarga = hargaJual * jumlah + pajak;

                                command.CommandText = @"
                                    INSERT INTO stok (tanggal, nama_pelanggan, merk_jenis, ukuran, jumlah, harga_jual, pajak, total_harga, keterangan)
                                    VALUES (@tanggal, @namaPelanggan, @merkJenis, @ukuran, @jumlah, @hargaJual, @pajak, @totalHarga, @keterangan)";
                                command.Parameters.AddWithValue("@tanggal", DateTime.Now);
                                command.Parameters.AddWithValue("@namaPelanggan", namaPelanggan);
                                command.Parameters.AddWithValue("@hargaJual", hargaJual);
                                command.Parameters.AddWithValue("@pajak", pajak);
                                command.Parameters.AddWithValue("@totalHarga", totalHarga);
                                command.Parameters.AddWithValue("@keterangan", "Pembelian");

                                command.ExecuteNonQuery();

                                Console.WriteLine($"Transaksi untuk produk {merkJenis} ukuran {ukuran} dengan jumlah {jumlah} berhasil diproses.");
                            }
                            else
                            {
                                Console.WriteLine($"Transaksi untuk produk {merkJenis} ukuran {ukuran} gagal karena stok tidak mencukupi.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Produk tidak ditemukan.");
                        }
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error processing transaction: " + ex.Message);
    }
}


        private static List<Produk> GetProductsFromDatabase(Database database)
        {
            var produkList = new List<Produk>();

            try
            {
                using (var connection = database.GetConnection())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT DISTINCT merk_jenis FROM barang";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var produk = new Produk
                                {
                                    MerkJenis = reader.GetString(0)
                                };
                                produkList.Add(produk);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving products from database: " + ex.Message);
            }

            return produkList;
        }

        private static List<string> GetSizesFromDatabase(Database database, string merkJenis)
        {
            var ukuranList = new List<string>();

            try
            {
                using (var connection = database.GetConnection())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT ukuran FROM barang WHERE merk_jenis = @merkJenis";
                        command.Parameters.AddWithValue("@merkJenis", merkJenis);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ukuranList.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving sizes from database: " + ex.Message);
            }

            return ukuranList;
        }

        public static void DisplayTransactions(Database database)
        {
            try
            {
                using (var connection = database.GetConnection())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM stok";

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("Tidak ada transaksi yang tersedia.");
                                return;
                            }

                            Console.WriteLine("==================================================================================================================================");
                            Console.WriteLine("| Tanggal               | Nama Pelanggan | Produk        | Ukuran | Jumlah | Harga Jual  | Pajak       | Total Harga | Keterangan |");
                            Console.WriteLine("==================================================================================================================================");

                            while (reader.Read())
                            {
                                Console.WriteLine($"| {((DateTime)reader["tanggal"]).ToString("yyyy-MM-dd HH:mm:ss"),-20} | " +
                                                  $"{reader["nama_pelanggan"],-13} | " +
                                                  $"{reader["merk_jenis"],-12} | " +
                                                  $"{reader["ukuran"],-6} | " +
                                                  $"{reader["jumlah"],-6} | " +
                                                  $"{((decimal)reader["harga_jual"]).ToString("C"),-11} | " +
                                                  $"{((decimal)reader["pajak"]).ToString("C"),-11} | " +
                                                  $"{((decimal)reader["total_harga"]).ToString("C"),-11} | " +
                                                  $"{reader["keterangan"],-10} |");
                            }

                            Console.WriteLine("==================================================================================================================================");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error displaying transactions: " + ex.Message);
            }
        }

        public static void SortTransactions(Database database)
        {
            try
            {
                using (var connection = database.GetConnection())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM stok ORDER BY tanggal DESC";

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("Tidak ada transaksi yang tersedia untuk diurutkan.");
                                return;
                            }

                            Console.WriteLine("==================================================================================================================================");
                            Console.WriteLine("| Tanggal               | Nama Pelanggan | Produk        | Ukuran | Jumlah | Harga Jual  | Pajak       | Total Harga | Keterangan |");
                            Console.WriteLine("==================================================================================================================================");

                            while (reader.Read())
                            {
                                Console.WriteLine($"| {((DateTime)reader["tanggal"]).ToString("yyyy-MM-dd HH:mm:ss"),-20} | " +
                                                  $"{reader["nama_pelanggan"],-13} | " +
                                                  $"{reader["merk_jenis"],-12} | " +
                                                  $"{reader["ukuran"],-6} | " +
                                                  $"{reader["jumlah"],-6} | " +
                                                  $"{((decimal)reader["harga_jual"]).ToString("C"),-11} | " +
                                                  $"{((decimal)reader["pajak"]).ToString("C"),-11} | " +
                                                  $"{((decimal)reader["total_harga"]).ToString("C"),-11} | " +
                                                  $"{reader["keterangan"],-10} |");
                            }

                            Console.WriteLine("==================================================================================================================================");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sorting transactions: " + ex.Message);
            }
        }

        public static void SearchTransactionsByJenisBarang(Database database, string merkJenis)
        {
            try
            {
                using (var connection = database.GetConnection())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM stok WHERE merk_jenis LIKE @merkJenis";
                        command.Parameters.AddWithValue("@merkJenis", "%" + merkJenis + "%");

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("Tidak ada transaksi yang sesuai dengan merk/jenis barang yang dicari.");
                                return;
                            }

                            Console.WriteLine("==================================================================================================================================");
                            Console.WriteLine("| Tanggal               | Nama Pelanggan | Produk        | Ukuran | Jumlah | Harga Jual  | Pajak       | Total Harga | Keterangan |");
                            Console.WriteLine("==================================================================================================================================");

                            while (reader.Read())
                            {
                                Console.WriteLine($"| {((DateTime)reader["tanggal"]).ToString("yyyy-MM-dd HH:mm:ss"),-20} | " +
                                                  $"{reader["nama_pelanggan"],-13} | " +
                                                  $"{reader["merk_jenis"],-12} | " +
                                                  $"{reader["ukuran"],-6} | " +
                                                  $"{reader["jumlah"],-6} | " +
                                                  $"{((decimal)reader["harga_jual"]).ToString("C"),-11} | " +
                                                  $"{((decimal)reader["pajak"]).ToString("C"),-11} | " +
                                                  $"{((decimal)reader["total_harga"]).ToString("C"),-11} | " +
                                                  $"{reader["keterangan"],-10} |");
                            }

                            Console.WriteLine("==================================================================================================================================");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error searching transactions: " + ex.Message);
            }
        }
    }
}