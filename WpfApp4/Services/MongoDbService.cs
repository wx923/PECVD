using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;
using WpfApp4.Models;
using System.Windows;
using System;
using System.Linq;

namespace WpfApp4.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Boat> _boats;
        private readonly IMongoCollection<BoatMonitor> _monitors;
        private readonly IMongoCollection<Process> _processes;
        private readonly IMongoCollection<ProcessReservation> _reservations;
        private readonly IMongoCollection<ProcessExcelModel> _processExcelCollection;
        private readonly IMongoCollection<ProcessFileInfo> _processFileCollection;

        public MongoDbService()
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
                settings.ConnectTimeout = TimeSpan.FromSeconds(5);

                var client = new MongoClient(settings);

                // 测试连接
                client.ListDatabaseNames().FirstOrDefault();

                _database = client.GetDatabase("PECVD");

                // 确保集合存在
                var collections = _database.ListCollectionNames().ToList();
                if (!collections.Contains("Boats"))
                    _database.CreateCollection("Boats");
                if (!collections.Contains("BoatMonitors"))
                    _database.CreateCollection("BoatMonitors");
                if (!collections.Contains("Processes"))
                    _database.CreateCollection("Processes");
                if (!collections.Contains("ProcessReservations"))
                    _database.CreateCollection("ProcessReservations");
                if (!collections.Contains("ProcessExcel"))
                    _database.CreateCollection("ProcessExcel");
                if (!collections.Contains("ProcessFiles"))
                    _database.CreateCollection("ProcessFiles");

                _boats = _database.GetCollection<Boat>("Boats");
                _monitors = _database.GetCollection<BoatMonitor>("BoatMonitors");
                _processes = _database.GetCollection<Process>("Processes");
                _reservations = _database.GetCollection<ProcessReservation>("ProcessReservations");
                _processExcelCollection = _database.GetCollection<ProcessExcelModel>("ProcessExcel");
                _processFileCollection = _database.GetCollection<ProcessFileInfo>("ProcessFiles");
            }
            catch (MongoConnectionException ex)
            {
                MessageBox.Show($"MongoDB连接失败: 请确保MongoDB服务已启动。\n详细信息: {ex.Message}");
                throw;
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"MongoDB连接超时: 请检查MongoDB服务是否正常运行。\n详细信息: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MongoDB初始化失败: {ex.GetType().Name}\n详细信息: {ex.Message}");
                throw;
            }
        }

        #region Boat Operations
        // 获取所有舟对象
        public async Task<List<Boat>> GetAllBoatsAsync()
        {
            try
            {
                return await _boats.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取舟对象失败: {ex.Message}");
                return new List<Boat>();
            }
        }

        // 添加舟对象
        public async Task AddBoatAsync(Boat boat)
        {
            try
            {
                await _boats.InsertOneAsync(boat);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加舟对象失败: {ex.Message}");
            }
        }

        // 更新舟对象
        public async Task<bool> UpdateBoatAsync(Boat boat)
        {
            try
            {
                // 查找现有记录
                var filter = Builders<Boat>.Filter.Eq("monitorBoatNumber", boat.MonitorBoatNumber);
                var existingBoat = await _boats.Find(filter).FirstOrDefaultAsync();

                if (existingBoat != null)
                {
                    // 使用现有记录的 id
                    boat._id = existingBoat._id;
                    // 更新记录
                    var updateFilter = Builders<Boat>.Filter.Eq("_id", boat._id);
                    var result = await _boats.ReplaceOneAsync(updateFilter, boat);
                    return result.ModifiedCount > 0;
                }
                else
                {
                    // 如果是新记录，生成新的 id
                    boat._id = ObjectId.GenerateNewId().ToString();
                    await _boats.InsertOneAsync(boat);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新舟对象失败: {ex.Message}");
                return false;
            }
        }

        // 删除舟对象
        public async Task DeleteBoatAsync(string id)
        {
            try
            {
                var filter = Builders<Boat>.Filter.Eq("_id", id);
                await _boats.DeleteOneAsync(filter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除舟对象失败: {ex.Message}");
            }
        }
        #endregion

        #region Boat Monitor Operations
        // 获取所有舟监控对象
        public async Task<List<BoatMonitor>> GetAllBoatMonitorsAsync()
        {
            try
            {
                return await _monitors.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取所有舟监控对象失败: {ex.Message}");
                return new List<BoatMonitor>();
            }
        }

        // 根据舟号获取监控对象
        public async Task<BoatMonitor> GetBoatMonitorByNumberAsync(string boatNumber)
        {
            try
            {
                return await _monitors.Find(x => x.BoatNumber == boatNumber).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"根据舟号获取获取监控对象失败: {ex.Message}");
                return null;
            }
        }

        // 添加监控对象
        public async Task<bool> AddBoatMonitorAsync(BoatMonitor monitor)
        {
            try
            {
                await _monitors.InsertOneAsync(monitor);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加监控对象失败: {ex.Message}");
                return false;
            }
        }

        // 更新监控对象
        public async Task<bool> UpdateBoatMonitorAsync(BoatMonitor monitor)
        {
            try
            {
                var filter = Builders<BoatMonitor>.Filter.Eq("_id", monitor._id);
                var result = await _monitors.ReplaceOneAsync(filter, monitor);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新监控对象失败: {ex.Message}");
                return false;
            }
        }

        // 删除监控对象
        public async Task<bool> DeleteBoatMonitorAsync(string id)
        {
            try
            {
                var filter = Builders<BoatMonitor>.Filter.Eq("_id", id);
                var result = await _monitors.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除监控对象失败: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Process Operations
        // 获取所有工艺
        public async Task<List<Process>> GetAllProcessesAsync()
        {
            try
            {
                return await _processes.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取工艺列表失败: {ex.Message}");
                return new List<Process>();
            }
        }

        // 添加工艺
        public async Task<bool> AddProcessAsync(Process process)
        {
            try
            {
                await _processes.InsertOneAsync(process);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加工艺失败: {ex.Message}");
                return false;
            }
        }

        // 更新工艺
        public async Task<bool> UpdateProcessAsync(Process process)
        {
            try
            {
                var filter = Builders<Process>.Filter.Eq(x => x.ProcessId, process.ProcessId);
                var result = await _processes.ReplaceOneAsync(filter, process);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新工艺失败: {ex.Message}");
                return false;
            }
        }

        // 删除工艺
        public async Task<bool> DeleteProcessAsync(string processId)
        {
            try
            {
                var filter = Builders<Process>.Filter.Eq(x => x.ProcessId, processId);
                var result = await _processes.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除工艺失败: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Process Excel Operations
        public async Task<List<ProcessExcelModel>> GetAllProcessExcelAsync()
        {
            try
            {
                return await _processExcelCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取工艺Excel数据失败: {ex.Message}");
            }
        }

        public async Task UpdateProcessExcelAsync(string fileId, List<ProcessExcelModel> processes)
        {
            try
            {
                // 先获取文件信息
                var fileInfo = await _processFileCollection.Find(x => x.Id == fileId).FirstOrDefaultAsync();
                if (fileInfo == null) throw new Exception("未找到工艺文件信息");

                // 获取对应的集合
                var collection = _database.GetCollection<ProcessExcelModel>(fileInfo.CollectionName);
                
                // 删除集合中的所有现有数据
                await collection.DeleteManyAsync(_ => true);

                // 插入新数据
                if (processes.Any())
                {
                    await collection.InsertManyAsync(processes);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"更新工艺Excel数据失败: {ex.Message}");
            }
        }
        #endregion

        #region Process Reservation Operations
        // 获取所有预约
        public async Task<List<ProcessReservation>> GetAllProcessReservationsAsync()
        {
            try
            {
                return await _reservations.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取预约列表失败: {ex.Message}");
                return new List<ProcessReservation>();
            }
        }

        // 添加预约
        public async Task<bool> AddProcessReservationAsync(ProcessReservation reservation)
        {
            try
            {
                await _reservations.InsertOneAsync(reservation);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加预约失败: {ex.Message}");
                return false;
            }
        }

        // 更新预约
        public async Task<bool> UpdateProcessReservationAsync(ProcessReservation reservation)
        {
            try
            {
                var filter = Builders<ProcessReservation>.Filter.Eq(x => x.Id, reservation.Id);
                var result = await _reservations.ReplaceOneAsync(filter, reservation);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新预约失败: {ex.Message}");
                return false;
            }
        }

        // 删除预约
        public async Task<bool> DeleteProcessReservationAsync(string id)
        {
            try
            {
                var filter = Builders<ProcessReservation>.Filter.Eq(x => x.Id, id);
                var result = await _reservations.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除预约失败: {ex.Message}");
                return false;
            }
        }
        #endregion

        // 保存新的工艺文件
        public async Task<string> SaveProcessFileAsync(string fileName, string description, List<ProcessExcelModel> data)
        {
            // 处理文件名以确保是有效的集合名（移除非法字符）
            string collectionName = fileName.Replace(" ", "_")  // 替换空格为下划线
                                          .Replace("-", "_")    // 替换横线为下划线
                                          .Replace(".", "_")    // 替换点为下划线
                                          .Replace("/", "_")    // 替换斜杠为下划线
                                          .Replace("\\", "_");  // 替换反斜杠为下划线

            var fileInfo = new ProcessFileInfo
            {
                FileName = fileName,
                CreateTime = DateTime.Now,
                Description = description,
                CollectionName = collectionName  // 使用处理后的文件名作为集合名
            };
            
            await _processFileCollection.InsertOneAsync(fileInfo);
            
            // 获取新的集合并插入数据
            var collection = _database.GetCollection<ProcessExcelModel>(fileInfo.CollectionName);
            await collection.InsertManyAsync(data);
            
            return fileInfo.Id;
        }

        // 获取所有工艺文件信息
        public async Task<List<ProcessFileInfo>> GetAllProcessFilesAsync()
        {
            return await _processFileCollection.Find(_ => true).ToListAsync();
        }

        // 获取指定工艺文件的数据
        public async Task<List<ProcessExcelModel>> GetProcessDataByFileIdAsync(string fileId)
        {
            // 先获取文件信息
            var fileInfo = await _processFileCollection.Find(x => x.Id == fileId).FirstOrDefaultAsync();
            if (fileInfo == null) return new List<ProcessExcelModel>();

            // 从对应的集合中获取数据
            var collection = _database.GetCollection<ProcessExcelModel>(fileInfo.CollectionName);
            return await collection.Find(_ => true).ToListAsync();
        }

        // 删除工艺文件及其数据
        public async Task DeleteProcessFileAsync(string fileId)
        {
            var fileInfo = await _processFileCollection.Find(x => x.Id == fileId).FirstOrDefaultAsync();
            if (fileInfo != null)
            {
                // 删除集合
                await _database.DropCollectionAsync(fileInfo.CollectionName);
                // 删除文件信息
                await _processFileCollection.DeleteOneAsync(x => x.Id == fileId);
            }
        }

        public async Task<bool> CollectionExistsAsync(string collectionName)
        {
            try
            {
                // 处理文件名以确保是有效的集合名（移除非法字符）
                string processedName = collectionName.Replace(" ", "_")  // 替换空格为下划线
                                                    .Replace("-", "_")    // 替换横线为下划线
                                                    .Replace(".", "_")    // 替换点为下划线
                                                    .Replace("/", "_")    // 替换斜杠为下划线
                                                    .Replace("\\", "_"); // 替换反斜杠为下划线

                var filter = new BsonDocument("name", processedName);
                var collections = await _database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
                return await collections.AnyAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"检查集合是否存在时出错: {ex.Message}");
            }
        }
    }
}