function previewImage(input){
    if(input.files && input.files[0]){
        var reader = new FileReader();
        reader.onload = function(e){
            var el = document.getElementById('profilePreview');
            if (el) el.src = e.target.result;
        }
        reader.readAsDataURL(input.files[0]);
    }
}

document.addEventListener('DOMContentLoaded', function(){
    const tabItems = document.querySelectorAll('.sidebar-nav-item');
    tabItems.forEach(function(item){
        item.addEventListener('click', function(e){
            e.preventDefault();
            const targetTab = this.getAttribute('data-tab');
            if (!targetTab) return;
            const url = new URL(window.location);
            url.searchParams.set('tab', targetTab);
            window.location.href = url.toString();
        });
    });

    const deleteModal = document.getElementById('deleteAccountModal');
    const openDeleteBtn = document.getElementById('openDeleteModal');
    const confirmInput = document.getElementById('confirmDeleteText');
    const confirmBtn = document.getElementById('confirmDeleteBtn');
    const closeTargets = deleteModal ? deleteModal.querySelectorAll('[data-delete-modal-close]') : [];

    function toggleConfirmButton(){
        if (!confirmBtn || !confirmInput) return;
        confirmBtn.disabled = confirmInput.value.trim() !== 'DELETE';
    }

    function openModal(){
        if (!deleteModal) return;
        deleteModal.classList.add('is-open');
        if (confirmInput){
            confirmInput.value = '';
            toggleConfirmButton();
            confirmInput.focus();
        }
    }

    function closeModal(){
        if (!deleteModal) return;
        deleteModal.classList.remove('is-open');
        if (confirmInput){
            confirmInput.value = '';
            toggleConfirmButton();
        }
    }

    if (confirmInput){
        confirmInput.addEventListener('input', toggleConfirmButton);
    }

    if (openDeleteBtn){
        openDeleteBtn.addEventListener('click', openModal);
    }

    closeTargets.forEach(function(el){
        el.addEventListener('click', closeModal);
    });

    if (deleteModal){
        deleteModal.addEventListener('click', function(evt){
            if (evt.target === deleteModal){
                closeModal();
            }
        });

        document.addEventListener('keydown', function(evt){
            if (evt.key === 'Escape' && deleteModal.classList.contains('is-open')){
                closeModal();
            }
        });
    }
});

// jQuery-dependent behaviors
$(document).on('click', '.btn-borrow-from-wishlist', function(){
    var bookId = $(this).data('book-id');
    $.ajax({ url: '/UserHome/AddToCart', type: 'POST', data: { bookId: bookId }, success: function(response){
        if (response.success){
            appModal.alert({ title: 'Success', message: 'Book added to cart!', variant: 'success' });
            window.location.href = '/UserHome/BookCheckout';
        } else {
            appModal.alert({ title: 'Error', message: response.message || 'Failed to add book to cart.', variant: 'error' });
        }
    }});
});

$(document).on('click', '.btn-remove-wishlist', async function(){
    var bookId = $(this).data('book-id');
    if(!(await appModal.confirm({ title: 'Remove from Wishlist', message: 'Remove this book from your wishlist?', okText: 'Remove', cancelText: 'Cancel', variant: 'warning' }))) return;
    $.ajax({ url: '/UserHome/ToggleWishlist', type: 'POST', data: { bookId: bookId }, success: function(response){
        if (response.success){ appModal.alert({ title: 'Success', message: 'Book removed from wishlist!', variant: 'success' }); window.location.reload(); }
        else { appModal.alert({ title: 'Error', message: response.message || 'Failed to remove book from wishlist.', variant: 'error' }); }
    }});
});

$(document).on('click', '.btn-cancel-reservation', async function(){
    var ticketId = $(this).data('ticket-id');
    if(!(await appModal.confirm({ title: 'Cancel Reservation', message: 'Cancel this reservation? This action cannot be undone.', okText: 'Cancel Reservation', cancelText: 'Keep', variant: 'warning' }))) return;
    $.ajax({ url: '/UserHome/CancelTicket', type: 'POST', data: { ticketId: ticketId }, success: function(response){
        if (response.success){ appModal.alert({ title: 'Success', message: 'Reservation cancelled successfully!', variant: 'success' }); window.location.reload(); }
        else { appModal.alert({ title: 'Error', message: response.message || 'Failed to cancel reservation.', variant: 'error' }); }
    }});
});

$(document).ready(function(){
    let originalBorrowedBooks = [], originalUnreturnedBooks = [], originalHistoryBooks = [], originalWishlistItems = [], originalPendingReservations = [];
    const itemsPerPage = { borrowed: 10, unreturned: 10, history: 15, wishlist: 12, pending: 10 };
    let currentPages = { borrowed: 1, unreturned: 1, history: 1, wishlist: 1, pending: 1 };

    function initializeOriginalData(){
        $('#borrowedBooksContainer .borrowed-book-card').each(function(){ originalBorrowedBooks.push({ element: $(this), title: $(this).find('h4').text().toLowerCase(), author: $(this).find('.book-author').text().toLowerCase(), dueDate: $(this).find('.due-date').text(), borrowedDate: $(this).find('.borrowed-date').text() }); });
        $('#unreturnedBooksContainer .overdue-book-card').each(function(){ originalUnreturnedBooks.push({ element: $(this), title: $(this).find('h4').text().toLowerCase(), author: $(this).find('.book-author').text().toLowerCase(), dueDate: $(this).find('.due-date').text(), daysOverdue: parseInt($(this).find('.days-overdue').text().match(/\d+/)?.[0] || '0'), fineAmount: parseFloat($(this).find('.fine-amount').text().replace(/[^\d.]/g, '') || '0') }); });
        $('#historyBooksContainer .history-book-item').each(function(){ originalHistoryBooks.push({ element: $(this), title: $(this).find('h4').text().toLowerCase(), author: $(this).find('.book-author').text().toLowerCase(), borrowedDate: $(this).find('.borrowed-date').text(), returnedDate: $(this).find('.returned-date').text() }); });
        $('#wishlistBooksContainer .wishlist-book-card').each(function(){ originalWishlistItems.push({ element: $(this), title: $(this).find('h4').text().toLowerCase(), author: $(this).find('.book-author').text().toLowerCase(), isAvailable: $(this).find('.btn-borrow-from-wishlist').length > 0 }); });
        $('#pending-reservations .pending-reservations-container .pending-reservation-card').each(function(){ originalPendingReservations.push({ element: $(this), title: $(this).find('h4').text().toLowerCase(), author: $(this).find('.reservation-author').text().toLowerCase(), reference: $(this).find('.reference-code').text().toLowerCase(), expiresAt: $(this).find('.expiry-date').text() }); });
    }

    function paginateItems(items, page, perPage){ const start = (page - 1) * perPage; return items.slice(start, start + perPage); }

    function renderPagination(totalItems, currentPage, perPage, containerId, filterFunction){
        const totalPages = Math.ceil(totalItems / perPage);
        const info = document.getElementById(containerId + 'PaginationInfo');
        const controls = document.getElementById(containerId + 'PaginationControls');
        if (totalPages <= 1){ if (info) info.textContent = `Showing ${totalItems} items`; if (controls) controls.innerHTML = ''; return; }
        if (info){ const startItem = (currentPage - 1) * perPage + 1; const endItem = Math.min(currentPage * perPage, totalItems); info.textContent = `Showing ${startItem}-${endItem} of ${totalItems} items (Page ${currentPage} of ${totalPages})`; }
        if (controls){ controls.innerHTML = ''; const pageNumbers = document.createElement('div'); pageNumbers.className = 'page-numbers';
            if (currentPage > 1){ const prevBtn = document.createElement('a'); prevBtn.href='#'; prevBtn.className='page-btn page-prev'; prevBtn.textContent='Previous'; prevBtn.addEventListener('click',function(e){e.preventDefault(); currentPages[containerId==='borrowed'?'borrowed':containerId] = currentPage - 1; filterFunction();}); pageNumbers.appendChild(prevBtn); }
            let startPage = Math.max(1, currentPage - 2), endPage = Math.min(totalPages, currentPage + 2);
            if (endPage - startPage < 4){ if (startPage === 1) endPage = Math.min(totalPages, startPage + 4); else if (endPage === totalPages) startPage = Math.max(1, endPage - 4); }
            if (startPage > 1){ const firstBtn = document.createElement('a'); firstBtn.href='#'; firstBtn.className='page-btn'; firstBtn.textContent='1'; firstBtn.addEventListener('click',function(e){e.preventDefault(); currentPages[containerId] = 1; filterFunction();}); pageNumbers.appendChild(firstBtn); if (startPage > 2){ const ellipsis = document.createElement('span'); ellipsis.className='page-ellipsis'; ellipsis.textContent='...'; pageNumbers.appendChild(ellipsis);} }
            for (let i = startPage; i <= endPage; i++){ const pageBtn = document.createElement('a'); pageBtn.href='#'; pageBtn.className = 'page-btn ' + (i===currentPage?'active':''); pageBtn.textContent = i; if (i!==currentPage){ pageBtn.addEventListener('click',function(e){e.preventDefault(); currentPages[containerId] = i; filterFunction();}); } pageNumbers.appendChild(pageBtn); }
            if (endPage < totalPages){ if (endPage < totalPages - 1){ const ellipsis2 = document.createElement('span'); ellipsis2.className='page-ellipsis'; ellipsis2.textContent='...'; pageNumbers.appendChild(ellipsis2);} const lastBtn = document.createElement('a'); lastBtn.href='#'; lastBtn.className='page-btn'; lastBtn.textContent=totalPages; lastBtn.addEventListener('click',function(e){e.preventDefault(); currentPages[containerId] = totalPages; filterFunction();}); pageNumbers.appendChild(lastBtn); }
            if (currentPage < totalPages){ const nextBtn = document.createElement('a'); nextBtn.href='#'; nextBtn.className='page-btn page-next'; nextBtn.textContent='Next'; nextBtn.addEventListener('click',function(e){e.preventDefault(); currentPages[containerId] = currentPage + 1; filterFunction();}); pageNumbers.appendChild(nextBtn); }
            controls.appendChild(pageNumbers);
            if (containerId === 'borrowed' || containerId === 'unreturned' || containerId === 'pending'){
                const itemsDiv = document.createElement('div'); itemsDiv.className='items-per-page'; itemsDiv.innerHTML = `
                    <select class="page-size-select" id="${containerId}ItemsPerPageSelect">
                        <option value="10" ${perPage===10?'selected':''}>10 / page</option>
                        <option value="15" ${perPage===15?'selected':''}>15 / page</option>
                        <option value="20" ${perPage===20?'selected':''}>20 / page</option>
                        <option value="25" ${perPage===25?'selected':''}>25 / page</option>
                    </select>`;
                itemsDiv.querySelector('#'+containerId+'ItemsPerPageSelect').addEventListener('change', function(){ itemsPerPage[containerId] = parseInt(this.value); currentPages[containerId] = 1; filterFunction(); });
                controls.appendChild(itemsDiv);
            }
            const goDiv = document.createElement('div'); goDiv.className='go-to-page'; goDiv.innerHTML = `
                <span class="go-to-label">Go to</span>
                <input type="number" class="page-input" min="1" max="${totalPages}" value="${currentPage}" id="${containerId}GotoPageInput" />
                <span class="go-to-label">Page</span>`;
            goDiv.querySelector('#'+containerId+'GotoPageInput').addEventListener('keypress', function(e){ if(e.key==='Enter'){ const page = Math.max(1, Math.min(parseInt(this.value)||1, totalPages)); currentPages[containerId] = page; filterFunction(); } });
            controls.appendChild(goDiv);
        }
    }

    function filterBorrowedBooks(){
        const searchTerm = ($('#borrowedSearchInput').val()||'').toLowerCase(); const sortBy = $('#borrowedSortSelect').val();
        let filtered = originalBorrowedBooks.filter(function(book){ return book.title.includes(searchTerm) || book.author.includes(searchTerm); });
        filtered.sort(function(a,b){ switch(sortBy){ case 'due-date': return new Date(a.dueDate) - new Date(b.dueDate); case 'latest-borrowed': return new Date(b.borrowedDate) - new Date(a.borrowedDate); case 'title-az': return a.title.localeCompare(b.title); case 'title-za': return b.title.localeCompare(a.title); default: return 0; } });
        const pageItems = paginateItems(filtered, currentPages.borrowed, itemsPerPage.borrowed);
        $('#borrowedBooksContainer').empty(); pageItems.forEach(function(book){ $('#borrowedBooksContainer').append(book.element); });
        $('#borrowedResultsInfo').text(`Showing ${filtered.length} of ${originalBorrowedBooks.length} books`);
        renderPagination(filtered.length, currentPages.borrowed, itemsPerPage.borrowed, 'borrowed', filterBorrowedBooks);
    }

    function filterUnreturnedBooks(){
        const searchTerm = ($('#unreturnedSearchInput').val()||'').toLowerCase(); const sortBy = $('#unreturnedSortSelect').val();
        let filtered = originalUnreturnedBooks.filter(function(book){ return book.title.includes(searchTerm) || book.author.includes(searchTerm); });
        filtered.sort(function(a,b){ switch(sortBy){ case 'most-overdue': return b.daysOverdue - a.daysOverdue; case 'least-overdue': return a.daysOverdue - b.daysOverdue; case 'highest-fine': return b.fineAmount - a.fineAmount; case 'lowest-fine': return a.fineAmount - b.fineAmount; case 'due-date': return new Date(a.dueDate) - new Date(b.dueDate); default: return 0; } });
        const pageItems = paginateItems(filtered, currentPages.unreturned, itemsPerPage.unreturned);
        $('#unreturnedBooksContainer').empty(); pageItems.forEach(function(book){ $('#unreturnedBooksContainer').append(book.element); });
        $('#unreturnedResultsInfo').text(`Showing ${filtered.length} of ${originalUnreturnedBooks.length} overdue books`);
        renderPagination(filtered.length, currentPages.unreturned, itemsPerPage.unreturned, 'unreturned', filterUnreturnedBooks);
    }

    function filterHistoryBooks(){
        const searchTerm = ($('#historySearchInput').val()||'').toLowerCase(); const sortBy = $('#historySortSelect').val();
        let filtered = originalHistoryBooks.filter(function(book){ return book.title.includes(searchTerm) || book.author.includes(searchTerm); });
        filtered.sort(function(a,b){ switch(sortBy){ case 'latest-return': return new Date(b.returnedDate) - new Date(a.returnedDate); case 'oldest-return': return new Date(a.returnedDate) - new Date(b.returnedDate); case 'latest-borrow': return new Date(b.borrowedDate) - new Date(a.borrowedDate); case 'title-az': return a.title.localeCompare(b.title); default: return 0; } });
        const pageItems = paginateItems(filtered, currentPages.history, itemsPerPage.history);
        $('#historyBooksContainer').empty(); pageItems.forEach(function(book){ $('#historyBooksContainer').append(book.element); });
        $('#historyResultsInfo').text(`Showing ${filtered.length} of ${originalHistoryBooks.length} history records`);
        renderPagination(filtered.length, currentPages.history, itemsPerPage.history, 'history', filterHistoryBooks);
    }

    function filterWishlistItems(){
        const searchTerm = ($('#wishlistSearchInput').val()||'').toLowerCase(); const availability = $('#wishlistAvailabilityFilter').val(); const sortBy = $('#wishlistSortSelect').val();
        let filtered = originalWishlistItems.filter(function(item){ const matchesSearch = item.title.includes(searchTerm) || item.author.includes(searchTerm); const matchesAvailability = availability==='all' || (availability==='available' && item.isAvailable) || (availability==='unavailable' && !item.isAvailable); return matchesSearch && matchesAvailability; });
        filtered.sort(function(a,b){ switch(sortBy){ case 'title-az': return a.title.localeCompare(b.title); case 'title-za': return b.title.localeCompare(a.title); case 'author-az': return a.author.localeCompare(b.author); default: return 0; } });
        const pageItems = paginateItems(filtered, currentPages.wishlist, itemsPerPage.wishlist);
        $('#wishlistBooksContainer').empty(); pageItems.forEach(function(item){ $('#wishlistBooksContainer').append(item.element); });
        $('#wishlistResultsInfo').text(`Showing ${filtered.length} of ${originalWishlistItems.length} wishlist items`);
        renderPagination(filtered.length, currentPages.wishlist, itemsPerPage.wishlist, 'wishlist', filterWishlistItems);
    }

    function filterPendingReservations(){
        const searchTerm = ($('#pendingSearchInput').val()||'').toLowerCase(); const sortBy = $('#pendingSortSelect').val();
        let filtered = originalPendingReservations.filter(function(item){ return item.title.includes(searchTerm) || item.author.includes(searchTerm) || item.reference.includes(searchTerm); });
        filtered.sort(function(a,b){ switch(sortBy){ case 'expiry-soonest': return new Date(a.expiresAt) - new Date(b.expiresAt); case 'expiry-latest': return new Date(b.expiresAt) - new Date(a.expiresAt); case 'title-az': return a.title.localeCompare(b.title); case 'title-za': return b.title.localeCompare(a.title); default: return 0; } });
        const pageItems = paginateItems(filtered, currentPages.pending, itemsPerPage.pending);
        $('#pending-reservations .pending-reservations-container').empty(); pageItems.forEach(function(item){ $('#pending-reservations .pending-reservations-container').append(item.element); });
        $('#pendingResultsInfo').text(`Showing ${filtered.length} of ${originalPendingReservations.length} reservations`);
        renderPagination(filtered.length, currentPages.pending, itemsPerPage.pending, 'pending', filterPendingReservations);
    }

    function clearBorrowedFilters(){ $('#borrowedSearchInput').val(''); $('#borrowedSortSelect').val('due-date'); currentPages.borrowed = 1; filterBorrowedBooks(); }
    function clearUnreturnedFilters(){ $('#unreturnedSearchInput').val(''); $('#unreturnedSortSelect').val('most-overdue'); currentPages.unreturned = 1; filterUnreturnedBooks(); }
    function clearHistoryFilters(){ $('#historySearchInput').val(''); $('#historyStartDate').val(''); $('#historyEndDate').val(''); $('#historySortSelect').val('latest-return'); currentPages.history = 1; filterHistoryBooks(); }
    function clearWishlistFilters(){ $('#wishlistSearchInput').val(''); $('#wishlistAvailabilityFilter').val('all'); $('#wishlistSortSelect').val('latest-added'); currentPages.wishlist = 1; filterWishlistItems(); }
    function clearPendingFilters(){ $('#pendingSearchInput').val(''); $('#pendingSortSelect').val('expiry-soonest'); currentPages.pending = 1; filterPendingReservations(); }

    $('#searchBorrowedBtn').on('click', function(){ currentPages.borrowed = 1; filterBorrowedBooks(); });
    $('#searchUnreturnedBtn').on('click', function(){ currentPages.unreturned = 1; filterUnreturnedBooks(); });
    $('#searchHistoryBtn').on('click', function(){ currentPages.history = 1; filterHistoryBooks(); });
    $('#searchWishlistBtn').on('click', function(){ currentPages.wishlist = 1; filterWishlistItems(); });
    $('#searchPendingBtn').on('click', function(){ currentPages.pending = 1; filterPendingReservations(); });
    $('#clearBorrowedFilters').on('click', clearBorrowedFilters);
    $('#clearUnreturnedFilters').on('click', clearUnreturnedFilters);
    $('#clearHistoryFilters').on('click', clearHistoryFilters);
    $('#clearWishlistFilters').on('click', clearWishlistFilters);
    $('#clearPendingFilters').on('click', clearPendingFilters);

    initializeOriginalData();
    filterBorrowedBooks();
    filterUnreturnedBooks();
    filterHistoryBooks();
    filterWishlistItems();
    filterPendingReservations();
});
