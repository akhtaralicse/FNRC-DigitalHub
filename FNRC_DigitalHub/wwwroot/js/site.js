$(function () {
    //$(document).ajaxStart(function (r) {
    //   // console.log(r);
    //    //console.log("ajax start");
       
    //});
    //$(document).ajaxSend(function (event, jqXHR, ajaxOptions) {
    //   // console.log("AJAX : ", ajaxOptions.url);
    //    if (ajaxOptions.url.indexOf("GetNotifications") < 0) {
    //        pleaseWait(true);
    //    }
    //    else {
            
    //    }
    //});
    //$(document).ajaxStop(function (r) {
    //    // console.log("ajax stop");
    //    console.log(r);
    //    pleaseWait(false);
    //});

    $('.owl-nav').hide();
});

function openLoginBox() {
    $.get("/Home/LoadLoginModal", function (data) {
        $("#modalContainer").html(data);
        var myModal = new bootstrap.Modal(document.getElementById('loginModal'));
        myModal.show();
    });
}
document.addEventListener('DOMContentLoaded', function () {
    const isInViewport = (element) => {
        const rect = element.getBoundingClientRect();
        return (
            rect.top >= 0 &&
            rect.left >= 0 &&
            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
            rect.right <= (window.innerWidth || document.documentElement.clientWidth)
        );
    }

    const fadeInSections = document.querySelectorAll('.fade-in-section');
    const animationText = document.querySelectorAll('.text-animation');
    //if (isInViewport(animationText)) {
    //    animationText.classList.add('animate');
    //}
    //else {
    //    animationText.classList.remove('animate');
    //}
    const observer = new IntersectionObserver(
        (entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    entry.target.classList.add('animate');
                } else {
                    entry.target.classList.remove('is-visible');
                    entry.target.classList.remove('animate');

                }
            });
        },
        {
            threshold: 0.5,
        }
    );
    const observerText = new IntersectionObserver(
        (entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate');
                } else {
                    entry.target.classList.remove('animate');

                }
            });
        },
        {
            threshold: 0.5,
        }
    );
    fadeInSections.forEach(section => {
        observer.observe(section);
    });

    animationText.forEach(section => {
        observerText.observe(section);
    });
});
function validateMobileNumber(input) {
    const regex = /^[0-9+]*$/;
    if (!regex.test(input.value)) {
        input.value = input.value.replace(/[^0-9+]/g, '');
        document.getElementById("error-message").innerText = "Only numbers and '+' are allowed.";
    } else {
        document.getElementById("error-message").innerText = "";
    }
}
function validateFileSize_Idea(inputElement) {
    const file = inputElement.files[0];
    if (!file) return false;
    var maxSizeMB = 10;
    const maxSizeBytes = maxSizeMB * 1024 * 1024;
    if (file.size > maxSizeBytes) {
        Swal.fire("Error", `File is too large! Maximum allowed: ${maxSizeMB}MB`, "error");

        inputElement.value = '';
        return false;
    }
    return true;
}
function validateFileSize(inputElement) {
    const file = inputElement.files[0];
    if (!file) return false;
    var maxSizeMB = 20;
    const maxSizeBytes = maxSizeMB * 1024 * 1024;
    if (file.size > maxSizeBytes) {
        Swal.fire("Error", `File is too large! Maximum allowed: ${maxSizeMB}MB`, "error");

        inputElement.value = '';
        return false;
    }
    return true;
}
function hideLoader() {
    document.getElementById("loader").style.display = "none";
    document.getElementById("content").style.display = "block";
}
function pleaseWait(act) {

    if (act) $('.loader-wrapper').addClass('show');
    else $('.loader-wrapper').removeClass('show');

};

function openNewApp(url) {
    window.location.href = url;
}

function truncateFileName(filename, maxLength = 10) {
    const dotIndex = filename.lastIndexOf(".");

    if (dotIndex === -1) return filename;

    const namePart = filename.substring(0, dotIndex);
    const extension = filename.substring(dotIndex);

    if (namePart.length > maxLength) {
        return namePart.substring(0, maxLength) + "..." + extension;
    }

    return filename; // Return original if it's within max length
}

function formInputs(controlName) {
    //return document.getElementsByClassName(controlName).length > 0
    //    ? document
    //        .getElementsByClassName(controlName)[0]
    //        .querySelectorAll("input, select, checkbox, textarea, radio, button")
    //    : null;

    if (document.getElementsByClassName(controlName).length <= 0)
        return null;
    else {
        var allControl = document
            .getElementsByClassName(controlName)[0]
            .querySelectorAll("input, select, checkbox, textarea, radio, button");
        const visibleControl = Array.from(allControl).filter(div => {
            const style = window.getComputedStyle(div);
            return style.display !== 'none';
        });
        return visibleControl;
    }
}
function validateCheckbox(radioList) {
    let isValid = false;

    if (radioList.length == 0) return true;

    radioList.forEach((e) => {
        let inputArr = document.getElementsByName(e);
        for (let i = 0; i < inputArr.length; i++) {

            if (inputArr[i].checked) {
                isValid = true;
                break;
            } else {
                if (inputArr[i].attributes.focused) {
                    inputArr[i].attributes.focused.value = "true";
                    isValid = false;
                }
            }
        }
    });
    return isValid;
}
function getCheckBoxList(inputs) {
    const radioBox = Array.from(inputs).filter((i) => i.type === "radio" && i.required === true);
    let radioList = [];
    radioBox.forEach((e) => {
        radioList.push(e.name);
    });
    return Array.from(new Set(radioList));
};
function validateForm(controlName) {
    let inputs = formInputs(controlName);
    if (inputs === null) return true;
    inputs = Array.from(inputs);

    const count = inputs.filter((i) => ((i.value === "" && i.required) || i.validity.patternMismatch || !i.validity.valid ||
        (i.type === "number" && i.min > i.value && i.required)) &&
        i.type !== "radio"
    );

    if (count.length > 0) {
        count.forEach((c) => {
            try {
                //console.log("Required--- " + c.name + "--- " + c.attributes?.label?.value);
                if (c.attributes.focused) c.attributes.focused.value = "true";
                else if (c.attributes["aria-invalid"]) c.attributes["aria-invalid"].value = "true";
            } catch (e) {
                console.log(e);
            }
        });
    }
    const isChecked = validateCheckbox(getCheckBoxList(inputs));
    if (count.length === 0 && isChecked) {
        return true;
    } else {
        document.getElementsByName(count[0].name)[0].focus();
        return false;
    }
};

function ResponseService(result, controlID) {
    if (result?.Status === false) {
        toastr.error(result.Message);
    }
    else if (result?.Status === true) {
        toastr.success(result.Message);
    }
    else {
        $("#" + controlID).html("");
        $("#" + controlID).html(result);
    }
}
function PostService(url, formData, successCallBack) {
    $.ajax({
        type: 'post',
        url: url,
        data: formData,
        //dataType: 'json',
        headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
        success: successCallBack,
        error: function (request, status, error) {
            console.log(request, status, error)
            Swal.fire("Error", "Something goes wrong. Try again later.", "error");
        }
    })
}
function FetchHttp(url, type, formData, successCallBack) {
    $.ajax({
        type: type,
        url: url,
        data: formData,
        dataType: 'json',
        headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
        success: function (response) {
            console.log(response);
            if (typeof successCallBack === 'function') {
                return successCallBack(response);
            } else {
                if (response.Status) {
                    Swal.fire("Success", response.Message, "success");
                }
            }
        },
        error: function (request, status, error) {
            Swal.fire("Error", error, "error");
        }
    })
}
function SuccessModal(title, text, lang) {
    Swal.fire({
        title: title,
        html: text,
        icon: "success",
        confirmButtonText: lang == 'ar' ? 'موافق' : "OK"
    }).then((result) => {
        if (result.isConfirmed) {

        }
    });
}
function WarningModal(title, text, lang) {
    Swal.fire({
        title: title,
        html: text,
        icon: "warning",
        confirmButtonText: lang == 'ar' ? 'موافق' : "OK"
    }).then((result) => {
        if (result.isConfirmed) {

        }
    });
}
function SuccessWithCallBack(title, text, lang, redirectURL) {
    Swal.fire({
        title: title,
        html: text,
        icon: "success",
        confirmButtonText: lang == 'ar' ? 'موافق' : "OK"
    }).then((result) => {
        if (result.isConfirmed) {

            openNewApp(redirectURL);
        }
    });
} function SuccessWithCallBackReload(title, text, lang, isReload) {
    Swal.fire({
        title: title,
        html: text,
        icon: "success",
        confirmButtonText: lang == 'ar' ? 'موافق' : "OK"
    }).then((result) => {
        if (result.isConfirmed) {
            if (isReload)
                window.location.reload(true);
        }
    });
}
function DeleteWithCallBack(lang, successCallBack) {
    Swal.fire({
        title: lang == 'ar' ? 'حذف' : 'Delete',
        text: lang == 'ar' ? "هل انت متأكد؟ هل تريد حذف هذا العنصر" : "Are you sure? you want to delete this item",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: lang == 'ar' ? 'نعم، تابع!' : 'Yes, proceed!',
        cancelButtonText: lang == 'ar' ? 'يلغي' : 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            successCallBack();
        }
        else if (result.dismiss === Swal.DismissReason.cancel) {

        }
    });
}


function ConfirmWithCallBack(textar, texten, lang, successCallBack) {
    Swal.fire({
        title: lang == 'ar' ? 'تأكيد' : 'Confirm',
        text: lang == 'ar' ? textar : texten,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: lang == 'ar' ? 'نعم، تابع!' : 'Yes, proceed!',
        cancelButtonText: lang == 'ar' ? 'يلغي' : 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            successCallBack();
        }
        else if (result.dismiss === Swal.DismissReason.cancel) {

        }
    });
}