(function() {
    let currentPage = 1;
    let pageSize = 20;
    let currentFilters = {
        searchQuery: '',
        startDate: null,
        endDate: null,
        statusFilter: 'all',
        fineStatusFilter: 'all',
        sortBy: 'latest'
    };

    const searchInput = document.getElementById('searchInput');
    const startDateInput = document.getElementById('startDate');
    const endDateInput = document.getElementById('endDate');
    const statusFilter = document.getElementById('statusFilter');
    const fineStatusFilter = document.getElementById('fineStatusFilter');
    const sortBy = document.getElementById('sortBy');
    const searchBtn = document.getElementById('searchBtn');
    const clearFiltersBtn = document.getElementById('clearFiltersBtn');
    const loadingIndicator = document.getElementById('loadingIndicator');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');
    const noResultsMessage = document.getElementById('noResultsMessage');
    const resultsTable = document.getElementById('resultsTable');
    const resultsInfo = document.getElementById('resultsInfo');
    const historyTableBody = document.getElementById('historyTableBody');
    const paginationContainer = document.getElementById('paginationContainer');
    const paginationInfo = document.getElementById('paginationInfo');
    const paginationControls = document.getElementById('paginationControls');

    if (searchBtn) searchBtn.addEventListener('click', function() { currentPage = 1; loadHistory(); });
    if (clearFiltersBtn) clearFiltersBtn.addEventListener('click', function() {
        if (searchInput) searchInput.value = '';
        if (startDateInput) startDateInput.value = '';
        if (endDateInput) endDateInput.value = '';
        if (statusFilter) statusFilter.value = 'all';
        if (fineStatusFilter) fineStatusFilter.value = 'all';
        if (sortBy) sortBy.value = 'latest';
        currentFilters = { searchQuery: '', startDate: null, endDate: null, statusFilter: 'all', fineStatusFilter: 'all', sortBy: 'latest' };
        currentPage = 1; loadHistory();
    });
    if (searchInput) searchInput.addEventListener('keypress', function(e) { if (e.key === 'Enter') { currentPage = 1; loadHistory(); } });

    function loadHistory() {
        currentFilters.searchQuery = (searchInput && searchInput.value.trim()) || '';
        currentFilters.startDate = (startDateInput && startDateInput.value) || null;
        currentFilters.endDate = (endDateInput && endDateInput.value) || null;
        currentFilters.statusFilter = (statusFilter && statusFilter.value) || 'all';
        currentFilters.fineStatusFilter = (fineStatusFilter && fineStatusFilter.value) || 'all';
        currentFilters.sortBy = (sortBy && sortBy.value) || 'latest';

        showLoading();

        const params = new URLSearchParams({ page: currentPage, pageSize: pageSize });
        if (currentFilters.searchQuery) params.append('searchQuery', currentFilters.searchQuery);
        if (currentFilters.startDate) params.append('startDate', currentFilters.startDate);
        if (currentFilters.endDate) params.append('endDate', currentFilters.endDate);
        if (currentFilters.statusFilter && currentFilters.statusFilter !== 'all') params.append('statusFilter', currentFilters.statusFilter);
        if (currentFilters.fineStatusFilter && currentFilters.fineStatusFilter !== 'all') params.append('fineStatusFilter', currentFilters.fineStatusFilter);
        if (currentFilters.sortBy && currentFilters.sortBy !== 'latest') params.append('sortBy', currentFilters.sortBy);

        fetch('/AdminHome/GetReturnHistory?' + params.toString())
            .then(function(response){ return response.json(); })
            .then(function(data){ if (data.success) { displayResults(data.data, data.pagination); } else { showError(data.message || 'Failed to load return history'); } })
            .catch(function(){ showError('Unable to load return history. Please try again.'); });
    }

    function showLoading(){ if (loadingIndicator) loadingIndicator.style.display = 'block'; if (errorMessage) errorMessage.style.display = 'none'; if (noResultsMessage) noResultsMessage.style.display = 'none'; if (resultsTable) resultsTable.style.display = 'none'; if (paginationContainer) paginationContainer.style.display = 'none'; }
    function showError(message){ if (loadingIndicator) loadingIndicator.style.display = 'none'; if (errorText) errorText.textContent = message; if (errorMessage) errorMessage.style.display = 'block'; if (noResultsMessage) noResultsMessage.style.display = 'none'; if (resultsTable) resultsTable.style.display = 'none'; if (paginationContainer) paginationContainer.style.display = 'none'; }

    function displayResults(data, pagination){
        if (loadingIndicator) loadingIndicator.style.display = 'none'; if (errorMessage) errorMessage.style.display = 'none';
        if (!data || data.length === 0){ if (noResultsMessage) noResultsMessage.style.display = 'block'; if (resultsTable) resultsTable.style.display = 'none'; if (paginationContainer) paginationContainer.style.display = 'none'; if (resultsInfo) resultsInfo.textContent = ''; return; }
        if (resultsTable) resultsTable.style.display = 'block'; if (noResultsMessage) noResultsMessage.style.display = 'none';
        if (resultsInfo){ const startRecord = (pagination.currentPage - 1) * pagination.pageSize + 1; const endRecord = Math.min(startRecord + data.length - 1, pagination.totalCount); resultsInfo.textContent = `Showing ${startRecord}-${endRecord} of ${pagination.totalCount} records`; }
        if (historyTableBody){ historyTableBody.innerHTML=''; data.forEach(function(item){ historyTableBody.appendChild(createHistoryRow(item)); }); }
        updatePagination(pagination);
    }

    function createHistoryRow(item){
        const row = document.createElement('tr');
        const durationText = item.duration > 0 ? `${item.duration} days` : 'N/A';
        const fineText = item.fineAmount > 0 ? `<span class="text-danger"><i class="fas fa-peso-sign"></i> &#8369;${item.fineAmount.toFixed(2)}</span>` : '<span class="text-success">No fine</span>';
        const statusBadge = `<span class="badge badge-success">${item.status}</span>`;
        const notesText = item.notes ? `<span class="text-truncate d-inline-block" style="max-width: 150px;" title="${item.notes}">${item.notes}</span>` : '<span class="text-muted">-</span>';
        row.innerHTML = `
            <td><code class="text-primary">${item.referenceCode}</code></td>
            <td><div><strong>${item.bookTitle}</strong>${item.bookAuthor ? `<br><small class="text-muted">by ${item.bookAuthor}</small>` : ''}</div></td>
            <td><i class="fas fa-user"></i> ${item.userName}</td>
            <td><i class="fas fa-calendar-plus"></i> ${item.checkedOutAt || 'N/A'}</td>
            <td><i class="fas fa-calendar-alt"></i> ${item.dueAt || 'N/A'}</td>
            <td><i class="fas fa-undo-alt"></i> ${item.returnedAt || 'N/A'}</td>
            <td><i class="fas fa-clock"></i> ${durationText}</td>
            <td>${fineText}</td>
            <td>${statusBadge}</td>
            <td>${notesText}</td>`;
        return row;
    }

    function updatePagination(pagination){
        if (!paginationContainer || pagination.totalPages <= 1){ if (paginationContainer) paginationContainer.style.display = 'none'; return; }
        paginationContainer.style.display = 'block';
        if (paginationInfo){ const startRecord = (pagination.currentPage - 1) * pagination.pageSize + 1; const endRecord = Math.min(startRecord + pagination.pageSize - 1, pagination.totalCount); paginationInfo.textContent = `Showing ${startRecord}-${endRecord} of ${pagination.totalCount} records (Page ${pagination.currentPage} of ${pagination.totalPages})`; }
        paginationControls.innerHTML = '';
        const pageNumbers = document.createElement('div'); pageNumbers.className='page-numbers';
        if (pagination.hasPrevious){ const prevBtn=document.createElement('a'); prevBtn.href='#'; prevBtn.className='page-btn page-prev'; prevBtn.textContent='Previous'; prevBtn.addEventListener('click',function(e){ e.preventDefault(); currentPage = pagination.currentPage - 1; loadHistory(); }); pageNumbers.appendChild(prevBtn); }
        let startPage = Math.max(1, pagination.currentPage - 2), endPage = Math.min(pagination.totalPages, pagination.currentPage + 2);
        if (endPage - startPage < 4){ if (startPage === 1) endPage = Math.min(pagination.totalPages, startPage + 4); else if (endPage === pagination.totalPages) startPage = Math.max(1, endPage - 4); }
        if (startPage > 1){ const firstBtn=document.createElement('a'); firstBtn.href='#'; firstBtn.className='page-btn'; firstBtn.textContent='1'; firstBtn.addEventListener('click',function(e){ e.preventDefault(); currentPage = 1; loadHistory(); }); pageNumbers.appendChild(firstBtn); if (startPage > 2){ const ellipsis=document.createElement('span'); ellipsis.className='page-ellipsis'; ellipsis.textContent='...'; pageNumbers.appendChild(ellipsis); } }
        for (let i=startPage;i<=endPage;i++){ const pageBtn=document.createElement('a'); pageBtn.href='#'; pageBtn.className='page-btn ' + (i===pagination.currentPage?'active':''); pageBtn.textContent=i; if (i!==pagination.currentPage){ pageBtn.addEventListener('click',function(e){ e.preventDefault(); currentPage=i; loadHistory(); }); } pageNumbers.appendChild(pageBtn); }
        if (endPage < pagination.totalPages){ if (endPage < pagination.totalPages - 1){ const ellipsis2=document.createElement('span'); ellipsis2.className='page-ellipsis'; ellipsis2.textContent='...'; pageNumbers.appendChild(ellipsis2); } const lastBtn=document.createElement('a'); lastBtn.href='#'; lastBtn.className='page-btn'; lastBtn.textContent=pagination.totalPages; lastBtn.addEventListener('click',function(e){ e.preventDefault(); currentPage=pagination.totalPages; loadHistory(); }); pageNumbers.appendChild(lastBtn); }
        if (pagination.hasNext){ const nextBtn=document.createElement('a'); nextBtn.href='#'; nextBtn.className='page-btn page-next'; nextBtn.textContent='Next'; nextBtn.addEventListener('click',function(e){ e.preventDefault(); currentPage=pagination.currentPage + 1; loadHistory(); }); pageNumbers.appendChild(nextBtn); }
        paginationControls.appendChild(pageNumbers);
        const itemsPerPageDiv = document.createElement('div'); itemsPerPageDiv.className='items-per-page'; itemsPerPageDiv.innerHTML = `
            <select class="page-size-select" id="itemsPerPageSelect">
                <option value="20" ${pagination.pageSize === 20 ? 'selected' : ''}>20 / page</option>
                <option value="25" ${pagination.pageSize === 25 ? 'selected' : ''}>25 / page</option>
                <option value="50" ${pagination.pageSize === 50 ? 'selected' : ''}>50 / page</option>
                <option value="100" ${pagination.pageSize === 100 ? 'selected' : ''}>100 / page</option>
            </select>`;
        itemsPerPageDiv.querySelector('#itemsPerPageSelect').addEventListener('change', function(){ pageSize = parseInt(this.value); currentPage = 1; loadHistory(); });
        paginationControls.appendChild(itemsPerPageDiv);
        const goToPageDiv = document.createElement('div'); goToPageDiv.className='go-to-page'; goToPageDiv.innerHTML = `
            <span class="go-to-label">Go to</span>
            <input type="number" class="page-input" min="1" max="${pagination.totalPages}" value="${pagination.currentPage}" id="gotoPageInput" />
            <span class="go-to-label">Page</span>`;
        goToPageDiv.querySelector('#gotoPageInput').addEventListener('keypress', function(e){ if (e.key==='Enter'){ const page = Math.max(1, Math.min(parseInt(this.value)||1, pagination.totalPages)); currentPage = page; loadHistory(); } });
        paginationControls.appendChild(goToPageDiv);
    }

    loadHistory();
})();
