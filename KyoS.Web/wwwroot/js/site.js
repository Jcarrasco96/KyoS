﻿// Oscar Hernández Baute
// Write your JavaScript code.

showInPopup = (url, title) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#form-modal .modal-body').html(res);
            $('#form-modal .modal-title').html(title);
            $('#form-modal').modal('show');
            // to make popup draggable
            $('.modal-dialog').draggable({
                handle: ".modal-header"
            });
        }
    })
}

showInPopupLg = (url, title) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#form-modal-lg .modal-body').html(res);
            $('#form-modal-lg .modal-title').html(title);
            $('#form-modal-lg').modal('show');
            // to make popup draggable
            $('.modal-dialog-lg').draggable({
                handle: ".modal-header"
            });
        }
    })
}

jQueryAjaxPost = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-diagnosis').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostDoc = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-documents').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostTem = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-templates').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostBillNote = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-templates').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "lengthMenu": [[100, 200, 400, -1], [100, 200, 400, "All"]],
                        "pageLength": 400
                    });        
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPaymentReceived = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-templates').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "lengthMenu": [[100, 200, 400, -1], [100, 200, 400, "All"]],
                        "pageLength": 400
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostGoal = form => {    
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-goals').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[1, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'MTPs/DeleteGoalOfAddendum';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostGoalMTPReview = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-goals').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[1, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'MTPs/DeleteGoalOfMTPreview';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostTCMSupervisor = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-templates').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "pageLength": 100
                    });           
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var url = 'TCMSupervisors/Delete';
                        window.location.href = url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostTCMClient = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-tcmClient').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var url = 'TCMClients/Delete';
                        window.location.href = url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostTCMService = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-tcmservices').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var url = 'TCMServices/Delete';
                        window.location.href = url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostTCMAdendums = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-tcmAdendum').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var url = 'TCMServicePlans/DeleteAdendum';
                        window.location.href = url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
        
}

jQueryAjaxPostTCMServicePlan = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-tcmServicePlan').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var url = 'TCMServicePlans/Delete';
                        window.location.href = url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMDischarge = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-tcmDischarge').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var url = 'TCMDischarges/Delete';
                        window.location.href = url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostBIOBehavioral = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-BioBehavioral').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'Bios/DeleteBehavioral';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostMedication = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-Medication').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'Medication/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostChangeNote = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-notes').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $.fn.dataTable.moment('MM/DD/YYYY');

                    $('#MyTable').DataTable({
                        "order": [[3, "asc"]],
                        "pageLength": 100
                    });

                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var url = 'Notes/Delete';
                        window.location.href = url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostChangeSessionByManager = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-notes').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $.fn.dataTable.moment('MM/DD/YYYY');

                    $('#MyTable').DataTable({
                        "order": [[3, "asc"]],
                        "pageLength": 100
                    });

                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var url = 'Notes/Delete';
                        window.location.href = url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMServicePlanReview = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-tcmServicePlanReview').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var url = 'TCMServicePlanReviews/DeleteDomain';
                        window.location.href = url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMAssessmentIndividualAgency = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-AssessmentIndividual').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMAssessment/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMAssessmentHouseComposition = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-Assessmenthouse').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMAssessment/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMAssessmentPastCurrent = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-AssessmentPast').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMAssessment/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMAssessmentMedication = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-AssessmentMedication').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMAssessment/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMAssessmenHospital = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-AssessmentHospital').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMAssessment/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMAssessmenDrug = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-AssessmentDrug').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMAssessment/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMAssessmenMedicalProblem = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-AssessmentMedicalProblem').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMAssessment/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostTCMAssessmenSurgery = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-AssessmentSurgery').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMAssessment/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}
jQueryAjaxPostTCMNoteActivity = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-NoteActivity').html(res.html)
                    $('#form-modal-lg .modal-body').html('');
                    $('#form-modal-lg .modal-title').html('');
                    $('#form-modal-lg').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMNotes/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal-lg .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}
jQueryAjaxPostTCMNoteActivityTemp = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-NoteActivityTemp').html(res.html)
                    $('#form-modal-lg .modal-body').html('');
                    $('#form-modal-lg .modal-title').html('');
                    $('#form-modal-lg').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMNotes/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal-lg .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}
jQueryAjaxPostTCMDomain = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-TCMdomain').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[1, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMServicePlans/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}
jQueryAjaxPostTCMDomainLg = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-TCMdomain').html(res.html)
                    $('#form-modal-lg .modal-body').html('');
                    $('#form-modal-lg .modal-title').html('');
                    $('#form-modal-lg').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[1, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'TCMServicePlans/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal-lg .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostReferred = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-referreds').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}
jQueryAjaxPostReferred1 = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-referreds').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostGoalTemp = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-goalsTemp').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[1, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'MTPs/DeleteGoalTemp';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostHealthInsurance = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-healthInsurance').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostBillNoteClient = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-templates').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[0, "asc"]],
                        "lengthMenu": [[100, 200, 400, -1], [100, 200, 400, "All"]],
                        "pageLength": 400
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostDeleteClientDiagnostic = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-ClientDiagnostics').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
          
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostAddClientDiagnostic = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-ClientDiagnostics').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    //$('#MyTable').DataTable({
                    //    "order": [[1, "asc"]],
                    //    "pageLength": 100
                    //});
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'ClientDiagnostics/Add';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }

}

jQueryAjaxPostDeleteGoalTemp = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-goalsTemp').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostSchedules = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-schedule').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[1, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'Schedules/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostGroupIndividual = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-group').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[1, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'Group/DeleteIndividual';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxPostEligibility = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-eligibility').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');

                    $('#MyTable').DataTable({
                        "order": [[1, "asc"]],
                        "pageLength": 100
                    });
                    var item_to_delete;
                    $('.deleteItem').click((e) => {
                        item_to_delete = e.currentTarget.dataset.id;
                    });
                    $("#btnYesDelete").click(function () {
                        var wwwUrlPath = window.document.location.href;
                        var pathName = window.document.location.pathname;
                        var pos = wwwUrlPath.indexOf(pathName);
                        var localhostPath = wwwUrlPath.substring(0, pos);
                        var url = 'Schedules/Delete';
                        window.location.href = localhostPath + '/' + url + '/' + item_to_delete;
                    });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}