using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class OrgBooksService : IOrgBooksService
    {
        private readonly IOrgBooksRepository _repository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IBookReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public OrgBooksService(IOrgBooksRepository repository, IOrganizationRepository organizationRepository, IBookReviewRepository reviewRepository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
            _organizationRepository = organizationRepository;
            _reviewRepository = reviewRepository;
        }

        public List<OrgBooksViewModel> GetAllBooks()
        {
            var books = _repository.GetAll().ToList();
            var viewModels = _mapper.Map<List<OrgBooksViewModel>>(books);

            // Use already-loaded Organization navigation property and add ratings
            foreach (var vm in viewModels)
            {
                var book = books.FirstOrDefault(b => b.Id == vm.Id);
                if (book?.Organization != null)
                {
                    vm.OrganizationName = book.Organization.Name;
                }
                
                // Add average rating
                vm.AverageRating = _reviewRepository.GetBookAverageRating(vm.Id);
            }

            return viewModels;
        }

        public List<OrgBooksViewModel> GetBookCatalog(int orgId)
        {
            List<OrgBooks> books;
            
            // If orgId is 0, return all active books from all organizations
            if (orgId == 0)
            {
                books = _repository.GetAll().Where(b => b.IsActive).ToList();
            }
            else
            {
                books = _repository.GetBooksByOrganization(orgId).ToList();
            }
            
            var viewModels = _mapper.Map<List<OrgBooksViewModel>>(books);

            // Use already-loaded Organization navigation property and add ratings
            foreach (var vm in viewModels)
            {
                var book = books.FirstOrDefault(b => b.Id == vm.Id);
                if (book?.Organization != null)
                {
                    vm.OrganizationName = book.Organization.Name;
                }
                
                // Add average rating
                vm.AverageRating = _reviewRepository.GetBookAverageRating(vm.Id);
            }

            return viewModels;
        }

        public List<OrgBooksViewModel> GetAllBooksByOrganization(int orgId)
        {
            var books = _repository.GetAllBooksByOrganization(orgId).ToList();
            var viewModels = _mapper.Map<List<OrgBooksViewModel>>(books);

            // Use already-loaded Organization navigation property
            foreach (var vm in viewModels)
            {
                var book = books.FirstOrDefault(b => b.Id == vm.Id);
                if (book?.Organization != null)
                {
                    vm.OrganizationName = book.Organization.Name;
                }
            }

            return viewModels;
        }

        public List<OrgBooksViewModel> GetAvailableBooks(int orgId)
        {
            var books = _repository.GetAvailableBooks(orgId).ToList();
            var viewModels = _mapper.Map<List<OrgBooksViewModel>>(books);

            // Use already-loaded Organization navigation property (all books have same org)
            if (books.Any() && books.First().Organization != null)
            {
                var orgName = books.First().Organization.Name;
                viewModels.ForEach(vm => vm.OrganizationName = orgName);
            }

            return viewModels;
        }

        public void AddBook(OrgBooksViewModel model)
        {
            // Validate ISBN uniqueness within organization
            if (!string.IsNullOrEmpty(model.ISBN) && _repository.BookExists(model.ISBN, model.OrganizationId))
            {
                throw new InvalidDataException($"A book with ISBN {model.ISBN} already exists in this organization.");
            }

            var book = _mapper.Map<OrgBooks>(model);
            book.CreatedTime = DateTime.Now;
            book.UpdatedTime = DateTime.Now;
            book.CreatedBy = System.Environment.UserName;
            book.UpdatedBy = System.Environment.UserName;
            book.IsActive = true;
            book.CopiesAvailable = model.CopiesTotal; // Initially all copies are available

            _repository.AddBook(book);
        }

        public void UpdateBook(OrgBooksViewModel model)
        {
            var existingBook = _repository.GetById(model.Id);
            if (existingBook == null)
            {
                throw new InvalidDataException("Book not found.");
            }

            // Map updates
            _mapper.Map(model, existingBook);
            existingBook.UpdatedTime = DateTime.Now;
            existingBook.UpdatedBy = System.Environment.UserName;

            _repository.UpdateBook(existingBook);
        }

        public OrgBooksViewModel GetBookById(int id)
        {
            var book = _repository.GetById(id);
            if (book == null)
            {
                return null;
            }

            var viewModel = _mapper.Map<OrgBooksViewModel>(book);
            
            // Use already-loaded Organization navigation property
            if (book.Organization != null)
            {
                viewModel.OrganizationName = book.Organization.Name;
            }

            // Add average rating
            viewModel.AverageRating = _reviewRepository.GetBookAverageRating(id);

            return viewModel;
        }

        public OrgBooksViewModel GetBookDetails(int bookId, int userId)
        {
            var book = _repository.GetById(bookId);
            if (book == null)
            {
                return null;
            }

            var viewModel = _mapper.Map<OrgBooksViewModel>(book);
            
            // Use already-loaded Organization navigation property
            if (book.Organization != null)
            {
                viewModel.OrganizationName = book.Organization.Name;
                viewModel.ContactNumber = book.Organization.ContactNumber;
                viewModel.Address = book.Organization.Address;
                
                // Get the first admin of the organization for display
                var admin = book.Organization.Admins?.FirstOrDefault();
                if (admin != null)
                {
                    viewModel.AdminName = admin.AdminUsername;
                    viewModel.AdminProfilePicture = admin.ProfilePicture;
                }
            }

            // Add average rating
            viewModel.AverageRating = _reviewRepository.GetBookAverageRating(bookId);

            // Add user's rating and review text
            var userReview = _reviewRepository.GetReviewByUserAndBook(userId, bookId);
            viewModel.UserRating = userReview?.Rating;
            viewModel.UserReviewText = userReview?.ReviewText;

            return viewModel;
        }

        public bool ValidateBookAvailability(int bookId)
        {
            var book = _repository.GetById(bookId);
            return book != null && book.IsActive && book.CopiesAvailable > 0;
        }

        public void ReserveCopy(int bookId)
        {
            if (!ValidateBookAvailability(bookId))
            {
                throw new InvalidOperationException("Book is not available for borrowing.");
            }

            _repository.DecrementCopies(bookId);
        }

        public void ReleaseCopy(int bookId)
        {
            _repository.IncrementCopies(bookId);
        }

        public void DeleteBook(int id)
        {
            var book = _repository.GetById(id);
            if (book == null)
            {
                throw new InvalidDataException("Book not found.");
            }

            // Check if book has any active borrows
            if (book.CopiesTotal != book.CopiesAvailable)
            {
                throw new InvalidOperationException("Cannot delete book with active borrows. Please wait for all copies to be returned.");
            }

            _repository.DeleteBook(id);
        }
    }
}


