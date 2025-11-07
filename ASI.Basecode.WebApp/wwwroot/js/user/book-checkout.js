// AppModal helper functions
function showAppModal(type, title, message, showCancel = false, onConfirm = null) {
    const modal = document.getElementById('appModal');
    const modalTitle = document.getElementById('appModalTitle');
    const modalMessage = document.getElementById('appModalMessage');
    const modalIcon = document.getElementById('appModalIcon');
    const modalOk = document.getElementById('appModalOk');
    const modalCancel = document.getElementById('appModalCancel');

    modal.querySelector('.modal-content').classList.remove('is-error', 'is-warning', 'is-info', 'is-success');

    let iconClass, variantClass;
    switch (type) {
        case 'error': iconClass = 'fas fa-circle-exclamation'; variantClass = 'is-error'; break;
        case 'warning': iconClass = 'fas fa-triangle-exclamation'; variantClass = 'is-warning'; break;
        case 'info': iconClass = 'fas fa-circle-info'; variantClass = 'is-info'; break;
        case 'success': iconClass = 'fas fa-circle-check'; variantClass = 'is-success'; break;
        default: iconClass = 'fas fa-circle-info'; variantClass = 'is-info';
    }

    modalTitle.textContent = title;
    modalMessage.innerHTML = message;
    modalIcon.className = iconClass;
    modal.querySelector('.modal-content').classList.add(variantClass);
    modalCancel.style.display = showCancel ? 'inline-block' : 'none';

    modalOk.onclick = function () {
        const bsModal = bootstrap.Modal.getInstance(modal) || new bootstrap.Modal(modal);
        bsModal.hide();
        if (onConfirm && typeof onConfirm === 'function') onConfirm();
    };
    modalCancel.onclick = function () {
        const bsModal = bootstrap.Modal.getInstance(modal) || new bootstrap.Modal(modal);
        bsModal.hide();
    };
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
}

function updateCheckoutCounters() {
    fetch('/UserHome/GetCheckoutCounters')
        .then(response => response.json())
        .then(data => {
            if (data && data.success) {
                const borrowedTodayElement = document.querySelector('.checkout-indicator-card:first-child .indicator-value');
                const borrowedTodayCaption = document.querySelector('.checkout-indicator-card:first-child .indicator-caption');
                if (borrowedTodayElement && borrowedTodayCaption) {
                    borrowedTodayElement.textContent = data.borrowedToday;
                    borrowedTodayCaption.textContent = `${data.borrowedToday} borrowed today`;
                }
                const activeTicketElement = document.querySelector('.checkout-indicator-card:nth-child(2) .indicator-value');
                const activeTicketCaption = document.querySelector('.checkout-indicator-card:nth-child(2) .indicator-caption');
                if (activeTicketElement && activeTicketCaption) {
                    activeTicketElement.textContent = data.maxActiveTickets > 0 ? `${data.activeTicketCount}/${data.maxActiveTickets}` : data.activeTicketCount;
                    activeTicketCaption.textContent = data.maxActiveTickets > 0 ? `${data.remainingActiveQuota} remaining from ${data.maxActiveTickets} max` : 'Keep track of your reserved books';
                }
            }
        })
        .catch(() => {});
}

function proceedWithCancellation(ticketId, buttonElement) {
    fetch('/UserHome/CancelBorrowTicket', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
        body: new URLSearchParams({ ticketId: ticketId }).toString()
    })
        .then(response => response.json())
        .then(result => {
            if (result && result.success) {
                handleCardRemoval(buttonElement);
            } else if (result && result.message) {
                showAppModal('error', 'Cancellation Failed', result.message, false);
            } else {
                showAppModal('error', 'Error', 'Unable to cancel reservation right now.', false);
            }
        })
        .catch(() => showAppModal('error', 'Error', 'Unable to cancel reservation right now.', false));
}

function proceedWithCartRemoval(bookId, buttonElement) {
    fetch('/UserHome/RemoveFromCart', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
        body: new URLSearchParams({ bookId: bookId }).toString()
    })
        .then(response => response.json())
        .then(result => {
            if (result && result.success) {
                handleCardRemoval(buttonElement);
                if (typeof updateCartBadge === 'function') updateCartBadge();
            } else {
                showAppModal('error', 'Error', 'Unable to remove book from cart right now.', false);
            }
        })
        .catch(() => showAppModal('error', 'Error', 'Unable to remove book from cart right now.', false));
}

function handleCardRemoval(buttonElement) {
    if (!buttonElement) { return; }
    if (typeof window !== 'undefined' && typeof window.bookCheckoutRemoveTicketCard === 'function') {
        window.bookCheckoutRemoveTicketCard(buttonElement);
    } else {
        const card = buttonElement.closest('.checkout-ticket-card');
        if (card) { card.remove(); }
    }
}

(function () {
    const selectAll = document.getElementById('selectAllTickets');
    const borrowButton = document.getElementById('borrowSelectedBtn');
    const selectedCount = document.getElementById('selectedCount');
    const ticketList = document.getElementById('checkoutTicketList');
    const emptyState = document.getElementById('emptyCheckoutState');
    const modal = document.getElementById('checkoutReferenceModal');
    const modalClose = document.getElementById('checkoutModalClose');
    const modalActionClose = document.getElementById('checkoutModalActionClose');
    const modalReferenceList = document.getElementById('checkoutModalReferenceList');
    const modalOrganization = document.getElementById('checkoutModalOrganization');

    function getTicketCheckboxes() { return document.querySelectorAll('.ticket-checkbox'); }
    function getCartItemCheckboxes() {
        const checkboxes = document.querySelectorAll('.cart-item-checkbox');
        return checkboxes;
    }
    function getAllCheckboxes() { return document.querySelectorAll('.ticket-checkbox, .cart-item-checkbox'); }

    function updateSelectedState() {
        const checkboxes = getAllCheckboxes();
        const checkedBoxes = Array.from(checkboxes).filter(cb => cb.checked);
        const hasSelection = checkedBoxes.length > 0;
        if (selectedCount) { selectedCount.textContent = checkedBoxes.length; }
        if (borrowButton) {
            borrowButton.disabled = !hasSelection;
            borrowButton.classList.toggle('is-active', hasSelection);
        }
        if (checkboxes.length === 0) { if (selectAll) { selectAll.checked = false; selectAll.disabled = true; } }
        else { if (selectAll) { selectAll.disabled = false; if (!selectAll.matches(':focus')) { selectAll.checked = checkedBoxes.length === checkboxes.length; } } }
    }

    function removeTicketCard(button) {
        const card = button.closest('.checkout-ticket-card');
        if (card) { card.remove(); }
        updateSelectedState();
        updateEmptyState();
    }
    window.bookCheckoutRemoveTicketCard = removeTicketCard;

    function attachCheckboxListeners() { getAllCheckboxes().forEach(cb => cb.addEventListener('change', updateSelectedState)); }
    function attachRemoveListeners() {
        const removeButtons = document.querySelectorAll('.checkout-remove-btn');
        removeButtons.forEach(btn => {
            if (btn.dataset.removeListenerAttached === 'true') { return; }
            btn.dataset.removeListenerAttached = 'true';
            btn.addEventListener('click', function () {
                const ticketId = this.dataset.ticketId;
                const bookId = this.dataset.bookId;
                if (ticketId) {
                    proceedWithCancellation(ticketId, this);
                } else if (bookId) {
                    proceedWithCartRemoval(bookId, this);
                }
            });
        });
    }

    function openModal(selections) {
        modalReferenceList.innerHTML = '';
        selections.forEach(ticket => {
            const item = document.createElement('div');
            item.className = 'reference-list-item';
            item.innerHTML = '\n                <div class="reference-list-content">\n                    <span class="reference-list-title">' + ticket.title + '</span>\n                    <span class="reference-list-code">' + ticket.reference + '</span>\n                </div>\n            ';
            modalReferenceList.appendChild(item);
        });
        if (selections.length > 0) modalOrganization.textContent = selections[0].organization;
        modal.classList.add('show');
    }
    function closeModal() { modal.classList.remove('show'); }

    function createPendingTicketCard(ticket) {
        const card = document.createElement('div');
        card.className = 'checkout-ticket-card';
        card.setAttribute('data-ticket-id', ticket.id);
        card.innerHTML = '\n            <div class="checkout-ticket-select">\n                <input type="checkbox" class="ticket-checkbox" value="' + ticket.id + '" data-reference="' + ticket.reference + '" data-title="' + ticket.title + '" data-organization="' + ticket.organization + '" />\n            </div>\n            <div class="checkout-ticket-info">\n                <div class="checkout-ticket-cover">\n                    <div class="checkout-ticket-placeholder">\n                        <i class="fas fa-book"></i>\n                    </div>\n                </div>\n                <div class="checkout-ticket-details">\n                    <h3>' + ticket.title + '</h3>\n                    <p class="ticket-author">by Author</p>\n                    <div class="ticket-meta">\n                        <span class="ticket-reference">\n                            <i class="fas fa-ticket-alt"></i>\n                            <span>Ref #: <strong>' + ticket.reference + '</strong></span>\n                        </span>\n                        <span class="ticket-expiry">\n                            <i class="fas fa-hourglass-half"></i>\n                            <span>Claim before ' + new Date(Date.now() + 24 * 60 * 60 * 1000).toLocaleString() + '</span>\n                        </span>\n                    </div>\n                </div>\n            </div>\n            <button type="button" class="checkout-remove-btn" data-ticket-id="' + ticket.id + '" title="Cancel reservation"><i class="fas fa-trash"></i></button>';
        attachCheckboxListeners();
        attachRemoveListeners();
        return card;
    }

    function updateEmptyState() {
        const cartItems = document.querySelectorAll('.cart-item-checkbox');
        const pendingTickets = document.querySelectorAll('.ticket-checkbox');
        if (cartItems.length === 0 && pendingTickets.length === 0 && emptyState) emptyState.style.display = 'flex';
        else if (emptyState) emptyState.style.display = 'none';
    }

    function initializeEventListeners() {
        attachCheckboxListeners();
        attachRemoveListeners();
        if (selectAll) {
            selectAll.addEventListener('change', function () {
                const checkboxes = getAllCheckboxes();
                const isChecked = this.checked;
                checkboxes.forEach(cb => { cb.checked = isChecked; });
                updateSelectedState();
            });
        }
        updateSelectedState();
    }

    if (borrowButton) {
        borrowButton.addEventListener('click', function () {
            const allCartCheckboxes = getCartItemCheckboxes();
            const allTicketCheckboxes = getTicketCheckboxes();
            const cartCheckboxes = Array.from(allCartCheckboxes).filter(cb => cb.checked);
            const ticketCheckboxes = Array.from(allTicketCheckboxes).filter(cb => cb.checked);
            if (!cartCheckboxes.length && !ticketCheckboxes.length) { showAppModal('warning', 'No Selection', 'No books selected to borrow.', false); return; }

            // Add loading state
            borrowButton.disabled = true;
            borrowButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
            borrowButton.classList.add('loading');

            if (cartCheckboxes.length > 0) {
                const cartBookIds = cartCheckboxes.map(cb => parseInt(cb.value));
                const formData = new URLSearchParams();
                cartBookIds.forEach(bookId => { formData.append('bookIds', bookId.toString()); });
                fetch('/UserHome/ConfirmBorrow', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                    body: formData.toString()
                })
                .then(response => response.json())
                .then(result => {
                    // Remove loading state
                    borrowButton.disabled = false;
                    borrowButton.innerHTML = 'BORROW NOW';
                    borrowButton.classList.remove('loading');

                    if (result && result.success) {
                        const selections = result.tickets.map(ticket => ({ id: ticket.ticketId, reference: ticket.referenceCode, title: ticket.bookTitle, organization: ticket.organizationName }));
                        openModal(selections);
                        cartCheckboxes.forEach(cb => {
                            const card = cb.closest('.checkout-ticket-card');
                            if (card) {
                                const found = selections.find(t => t.title === cb.dataset.title);
                                if (found) {
                                    const newTicketCard = createPendingTicketCard(found);
                                    const pendingSection = document.querySelector('#checkoutTicketList');
                                    if (pendingSection) pendingSection.appendChild(newTicketCard);
                                }
                                card.remove();
                            }
                        });
                        if (typeof updateCartBadge === 'function') updateCartBadge();
                        updateEmptyState();
                        updateCheckoutCounters();
                    } else {
                        let errorMessage = result.message || 'Failed to create borrow requests.';
                        if (result.errors && result.errors.length > 0) errorMessage += '\n\nDetails:\n' + result.errors.join('\n');
                        showAppModal('error', 'Borrow Failed', errorMessage.replace(/\n/g, '<br>'), false);
                    }
                })
                .catch(() => { 
                    // Remove loading state on error
                    borrowButton.disabled = false;
                    borrowButton.innerHTML = 'BORROW NOW';
                    borrowButton.classList.remove('loading');
                    showAppModal('error', 'Error', 'Unable to process borrow request right now.', false); 
                });
            } else {
                // Remove loading state for ticket-only selections
                borrowButton.disabled = false;
                borrowButton.innerHTML = 'BORROW NOW';
                borrowButton.classList.remove('loading');
                const selections = ticketCheckboxes.map(cb => ({ id: cb.value, reference: cb.dataset.reference, title: cb.dataset.title, organization: cb.dataset.organization }));
                openModal(selections);
            }
        });
    }

    if (modalClose) { modalClose.addEventListener('click', closeModal); }
    if (modalActionClose) { modalActionClose.addEventListener('click', closeModal); }
    modal.addEventListener('click', function (event) { if (event.target === modal) closeModal(); });

    setTimeout(function(){ initializeEventListeners(); }, 100);
})();
