using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Areas.Equipment.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Equipment.Controllers
{
    [Area("Equipment")]
    [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class DevicesController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDeviceService _deviceService;

        public DevicesController(IUnitOfWork unitOfWork, IDeviceService deviceService)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _deviceService = deviceService;
        }

        // GET: Device/Devices
        public IActionResult Index()
        {
            return RedirectToAction("Active");
        }

        public async Task<IActionResult> Active()
        {
            var devices = await _unitOfWork.Devices.ForListAsync(device => device.Status < Status.WrittenOffWithdrawn);

            var viewModel = await CreateViewModel<Devices_ViewModel>();
            viewModel.Devices = devices.Select(Devices_ViewModel.DeviceDto.ConvertFromDevice).ToList();
            viewModel.ListOfMakes = await _unitOfWork.Devices.ListOfMakesAsync();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> All()
        {
            var devices = await _unitOfWork.Devices.ForListAsync(device => true);

            var viewModel = await CreateViewModel<Devices_ViewModel>();
            viewModel.Devices = devices.Select(Devices_ViewModel.DeviceDto.ConvertFromDevice).ToList();
            viewModel.ListOfMakes = await _unitOfWork.Devices.ListOfMakesAsync();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Inactive()
        {
            var devices = await _unitOfWork.Devices.ForListAsync(device => device.Status >= Status.WrittenOffWithdrawn);

            var viewModel = await CreateViewModel<Devices_ViewModel>();
            viewModel.Devices = devices.Select(Devices_ViewModel.DeviceDto.ConvertFromDevice).ToList();
            viewModel.ListOfMakes = await _unitOfWork.Devices.ListOfMakesAsync();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithStatus(int id)
        {
            var devices = await _unitOfWork.Devices.ForListAsync(device => device.Status == (Status)id);

            var viewModel = await CreateViewModel<Devices_ViewModel>();
            viewModel.Devices = devices.Select(Devices_ViewModel.DeviceDto.ConvertFromDevice).ToList();
            viewModel.ListOfMakes = await _unitOfWork.Devices.ListOfMakesAsync();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> OfMake(string make)
        {
            var devices = await _unitOfWork.Devices.ForListAsync(device => device.Make == make);

            var viewModel = await CreateViewModel<Devices_ViewModel>();
            viewModel.Devices = devices.Select(Devices_ViewModel.DeviceDto.ConvertFromDevice).ToList();
            viewModel.ListOfMakes = await _unitOfWork.Devices.ListOfMakesAsync();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Search(string[] make, int[] status)
        {
            var filteredDevices = new List<Device>();

            if (status.Length != 0)
            {
                filteredDevices.AddRange(await _unitOfWork.Devices.ForListAsync(device => status.Contains((int)device.Status)));
            }
            
            if (make.Length != 0 && filteredDevices.Any())
            {
                filteredDevices = filteredDevices.Where(device => make.Contains(device.Make)).ToList();
            }
            else if (make.Length != 0)
            {
                filteredDevices.AddRange(await _unitOfWork.Devices.ForListAsync(device => make.Contains(device.Make)));
            }

            var viewModel = await CreateViewModel<Devices_ViewModel>();
            viewModel.Devices = filteredDevices.Select(Devices_ViewModel.DeviceDto.ConvertFromDevice).ToList();
            viewModel.ListOfMakes = await _unitOfWork.Devices.ListOfMakesAsync();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> ReportUsage()
        {
            var viewModel = await CreateViewModel<Devices_ReportsStatus>();
            var devices = await _unitOfWork.Devices.ForReportingAsync();
            var makes = devices.Select(d => d.Make).Distinct();

            var statuses = new Dictionary<string, int[]>
            {
                {"Unknown", new int[] {0}},
                {"Ready", new int[] {1, 3}},
                {"Returning", new int[] {4}},
                {"Repairing", new int[] {5, 6, 7}},
                {"On Hold", new int[] {8}},
                {"Written Off", new int[] {9, 10, 11, 12}}
            };

            foreach (var make in makes)
            {
                var device = new Devices_ReportsModel
                {
                    Name = make
                };

                var allocatedDetail = new Devices_ReportsDetail
                {
                    Status = "Allocated",
                    UnallocatedValue = devices.Where(d => d.Make == make).Count(d => d.IsAllocated()),
                    AllocatedValue = 0
                };

                device.Details.Add(allocatedDetail);

                foreach (var status in statuses)
                {
                    var reportStatus = new Devices_ReportsDetail
                    {
                        Status = status.Key,
                        UnallocatedValue = devices.Where(d => d.Make == make && !d.IsAllocated()).Count(d => status.Value.Any(e => e == (int)d.Status)),
                        AllocatedValue = devices.Where(d => d.Make == make && d.IsAllocated()).Count(d => status.Value.Any(e => e == (int)d.Status))
                    };

                    device.Details.Add(reportStatus);
                }

                var totalDetail = new Devices_ReportsDetail
                {
                    Status = "Total",
                    UnallocatedValue = devices.Count(d => d.Make == make),
                    AllocatedValue = 0
                };

                device.Details.Add(totalDetail);

                viewModel.Models.Add(device);
            }

            viewModel.StatusList.Add("Allocated");

            foreach (var status in statuses)
            {
                viewModel.StatusList.Add(status.Key);
            }

            viewModel.StatusList.Add("Total");

            return View("Report", viewModel);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var device = await _unitOfWork.Devices.ForDetailDisplayAsync(id);

            if (device == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = await CreateViewModel<Devices_DetailsViewModel>();
            viewModel.Device = Devices_DetailsViewModel.DeviceDto.ConvertFromDevice(device);
            viewModel.Allocations = device.Allocations.Select(Devices_DetailsViewModel.AllocationDto.ConvertFromAllocation).ToList();
            viewModel.Notes = device.Notes.Select(Devices_DetailsViewModel.NoteDto.ConvertFromNote).ToList();

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> Deallocate(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            var device = await _unitOfWork.Devices.ForEditAsync(id);

            if (device == null)
            {
                return RedirectToAction("Index");
            }

            if (!device.IsAllocated())
            {
                return RedirectToAction("Index");
            }

            await _deviceService.DeallocateDevice(device.SerialNumber);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", new { id = device.SerialNumber });
        }

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> Allocate(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            var device = await _unitOfWork.Devices.ForEditAsync(id);

            if (device == null)
            {
                return RedirectToAction("Index");
            }

            if (device.IsAllocated())
            {
                return RedirectToAction("Index");
            }

            var students = await _unitOfWork.Students.ForSelectionListAsync();

            var viewModel = await CreateViewModel<Devices_AssignViewModel>();
            viewModel.SerialNumber = device.SerialNumber;
            viewModel.Model = device.Model;
            viewModel.Make = device.Make;
            viewModel.Description = device.Description;
            viewModel.Status = device.Status;
            viewModel.StudentList = new SelectList(students, "StudentId", "DisplayName");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> Allocate(Devices_AssignViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.StudentId))
                ModelState.AddModelError("StudentId", "You must select a Student!");

            var device = await _unitOfWork.Devices.ForEditAsync(viewModel.SerialNumber);

            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);

                //Recreate viewModel!
                viewModel.SerialNumber = device.SerialNumber;
                viewModel.Model = device.Model;
                viewModel.Make = device.Make;
                viewModel.Description = device.Description;
                viewModel.Status = device.Status;
                viewModel.StudentList = new SelectList(await _unitOfWork.Students.ForSelectionListAsync(), "StudentId", "DisplayName");

                return View("Allocate", viewModel);
            }

            var student = await _unitOfWork.Students.ForEditAsync(viewModel.StudentId);

            if (student == null)
            {
                return RedirectToAction("Details", new { id = device.SerialNumber });
            }

            await _deviceService.AllocateDevice(device.SerialNumber, student.StudentId);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", new { id = device.SerialNumber });
        }

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> StatusUpdate(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            var device = await _unitOfWork.Devices.ForEditAsync(id);

            if (device == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = await CreateViewModel<Devices_StatusUpdateViewModel>();
            viewModel.SerialNumber = device.SerialNumber;
            viewModel.Model = device.Model;
            viewModel.Make = device.Make;
            viewModel.Description = device.Description;
            viewModel.CurrentStatus = device.Status;
            viewModel.GenerateStatusList(viewModel.CurrentStatus);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> StatusUpdate(Devices_StatusUpdateViewModel viewModel)
        {
            var device = await _unitOfWork.Devices.ForEditAsync(viewModel.SerialNumber);

            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);

                //Recreate viewModel!
                viewModel.SerialNumber = device.SerialNumber;
                viewModel.Model = device.Model;
                viewModel.Make = device.Make;
                viewModel.Description = device.Description;
                viewModel.CurrentStatus = device.Status;

                return View("StatusUpdate", viewModel);
            }

            await _deviceService.UpdateDeviceStatus(device.SerialNumber, viewModel.NewStatus, viewModel.Notes);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", new { id = device.SerialNumber });
        }

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> AddNote(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            var device = await _unitOfWork.Devices.ForEditAsync(id);

            if (device == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = await CreateViewModel<Devices_CreateNoteViewModel>();
            viewModel.SerialNumber = device.SerialNumber;
            viewModel.Model = device.Model;
            viewModel.Make = device.Make;
            viewModel.Description = device.Description;
            viewModel.Status = device.Status;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> AddNote(Devices_CreateNoteViewModel viewModel)
        {
            var device = await _unitOfWork.Devices.ForEditAsync(viewModel.SerialNumber);

            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);

                //Recreate viewModel!
                viewModel.SerialNumber = device.SerialNumber;
                viewModel.Model = device.Model;
                viewModel.Make = device.Make;
                viewModel.Description = device.Description;
                viewModel.Status = device.Status;

                return View("AddNote", viewModel);
            }

            await _deviceService.AddDeviceNote(device.SerialNumber, viewModel.Notes);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", new { id = device.SerialNumber });
        }

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> Create()
        {
            var viewModel = await CreateViewModel<Devices_UpdateViewModel>();
            viewModel.Device = new DeviceResource();
            viewModel.IsNew = true;

            return View("Update", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            var device = await _unitOfWork.Devices.ForEditAsync(id);

            if (device == null)
            {
                return RedirectToAction("Index");
            }

            if (device.Status >= Status.WrittenOffWithdrawn)
            {
                return RedirectToAction("Index");
            }

            var viewModel = await CreateViewModel<Devices_UpdateViewModel>();
            viewModel.Device = new DeviceResource
            {
                SerialNumber = device.SerialNumber,
                Model = device.Model,
                Description = device.Description,
                Make = device.Make,
                DateWarrantyExpires = device.DateWarrantyExpires,
                Status = device.Status
            };
            viewModel.IsNew = false;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor)]
        public async Task<IActionResult> Update(Devices_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);
                return View("Update", viewModel);
            }

            if (viewModel.IsNew)
            {
                await _deviceService.CreateDevice(viewModel.Device);
            }
            else
            {
                await _deviceService.UpdateDevice(viewModel.Device.SerialNumber, viewModel.Device);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }
    }
}