document.addEventListener('DOMContentLoaded', function(){
    var fileInput = document.getElementById('profilePictureFile');
    if (fileInput){
        fileInput.addEventListener('change', function(e){
            var file = e.target.files && e.target.files[0];
            if (!file) return;
            var reader = new FileReader();
            reader.onload = function(ev){
                var preview = document.getElementById('profilePreview');
                if (preview){ preview.innerHTML = '<img src="' + ev.target.result + '" alt="Profile" class="profile-preview" />'; }
            };
            reader.readAsDataURL(file);
        });
    }
    var deleteModal = document.getElementById('deleteAccountModal');
    var openDeleteBtn = document.getElementById('openDeleteModal');
    var confirmInput = document.getElementById('confirmDeleteText');
    var confirmBtn = document.getElementById('confirmDeleteBtn');
    var closeTargets = deleteModal ? deleteModal.querySelectorAll('[data-delete-modal-close]') : [];

    function toggleConfirmButton(){
        if (!confirmInput || !confirmBtn) return;
        confirmBtn.disabled = confirmInput.value.trim() !== 'DELETE';
    }

    function openModal(){
        if (!deleteModal) return;
        deleteModal.classList.add('is-open');
        if (confirmInput){ confirmInput.focus(); }
    }

    function closeModal(){
        if (!deleteModal) return;
        deleteModal.classList.remove('is-open');
        if (confirmInput){
            confirmInput.value = '';
            toggleConfirmButton();
        }
    }

    if (confirmInput){ confirmInput.addEventListener('input', toggleConfirmButton); }
    if (openDeleteBtn){ openDeleteBtn.addEventListener('click', openModal); }
    closeTargets.forEach(function(el){ el.addEventListener('click', closeModal); });

    if (deleteModal){
        deleteModal.addEventListener('click', function(evt){
            if (evt.target === deleteModal) closeModal();
        });
        document.addEventListener('keydown', function(evt){
            if (evt.key === 'Escape' && deleteModal.classList.contains('is-open')){
                closeModal();
            }
        });
    }
});
